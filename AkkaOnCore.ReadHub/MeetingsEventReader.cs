using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AkkaOnCore.Messages;
using AkkaOnCore.ReadModel.Meetings;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AkkaOnCore.ReadHub
{
	public class MeetingsEventReader : BackgroundService, IEventConnection
	{
		private readonly IHubContext<MeetingsHub, IMeetings> _meetingsHub;
		private readonly ILogger<MeetingsEventReader> _logger;

		private bool _isConnected;
		private readonly IEventStoreConnection _connection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@eventstore-node:1113; MaxReconnections=-1");

		private readonly List<IProcessEvents> _eventProcessors = new List<IProcessEvents>();

		private readonly MeetingsListReadModel _readModel = new MeetingsListReadModel();

		public MeetingsEventReader(IHubContext<MeetingsHub, IMeetings> meetingsHub, ILogger<MeetingsEventReader> logger)
		{
			_meetingsHub = meetingsHub;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken cancelToken)
		{
			try
			{
				_connection.Connected += OnConnected;
				_connection.Disconnected += OnDisconnected;
				_connection.Closed += OnClosed;
				_connection.ErrorOccurred += OnErrorOccurred;

				await _connection.ConnectAsync();

				_logger.LogDebug("After ConnectAsync()");

				_eventProcessors.Add(new EventProcessor<MeetingsEvent>(new EventReader<MeetingsEvent>(this, "MeetingsActor"), HandleMeetingsEvent));

				while (!cancelToken.IsCancellationRequested)
				{
					foreach (var processor in _eventProcessors.ToArray())
					{
						await processor.Process();
					}

					await Task.Delay(TimeSpan.FromMilliseconds(10), cancelToken);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		public async Task<IEnumerable<ResolvedEvent>> ReadEvents(string id, long start, int count)
			=> _isConnected 
				? (await _connection.ReadStreamEventsForwardAsync(id, start, count, false)).Events.AsEnumerable()
				: Enumerable.Empty<ResolvedEvent>();

		private Task HandleMeetingsEvent(MeetingsEvent meetingsEvent)
		{
			if (meetingsEvent is MeetingsEvent.MeetingStartedEvent meetingStarted)
				_eventProcessors.Add(new EventProcessor<MeetingEvent>(new EventReader<MeetingEvent>(this, $"Meeting-{meetingStarted.MeetingId}"), HandleMeetingEvent));

			return SendReadModelEvents(_readModel.Integrate(meetingsEvent));
		}

		private Task HandleMeetingEvent(MeetingEvent meetingEvent)
			=> SendReadModelEvents(_readModel.Integrate(meetingEvent));

		private async Task SendReadModelEvents(IEnumerable<MeetingsListEvent> events)
		{
			foreach (var @event in events)
				await SendReadModelEvent(@event);
		}

		private Task SendReadModelEvent(MeetingsListEvent @event)
			=> @event.Match(
				meetingAdded => _meetingsHub.Clients.All.OnMeetingAddedToList(meetingAdded.MeetingId, meetingAdded.Name),
				agendaItemCountChanged => _meetingsHub.Clients.All.OnMeetingAgendaCountChanged(agendaItemCountChanged.MeetingId, agendaItemCountChanged.NewCount)
			);

		private void OnErrorOccurred(object sender, ClientErrorEventArgs e)
		{
			_logger.LogError(e.Exception.ToString());
		}

		private void OnClosed(object sender, ClientClosedEventArgs e)
		{
			_logger.LogInformation("Closed");
		}

		private void OnDisconnected(object sender, ClientConnectionEventArgs e)
		{
			_logger.LogInformation("Disconnected");
			_isConnected = false;
		}

		private void OnConnected(object sender, ClientConnectionEventArgs e)
		{
			_logger.LogInformation("Connected");
			_isConnected = true;
		}
	}
}