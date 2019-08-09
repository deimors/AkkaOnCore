using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AkkaOnCore.Messages;
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
		private readonly IEventStoreConnection _connection;

		private readonly List<IProcessEvents> _eventProcessors = new List<IProcessEvents>();

		private readonly Dictionary<Guid, int> _agendaCounts = new Dictionary<Guid, int>();

		public MeetingsEventReader(IHubContext<MeetingsHub, IMeetings> meetingsHub, ILogger<MeetingsEventReader> logger)
		{
			_meetingsHub = meetingsHub;
			_logger = logger;
			_connection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@eventstore-node:1113; MaxReconnections=-1");
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
			=> meetingsEvent.Match(
				meetingStarted =>
				{
					_eventProcessors.Add(new EventProcessor<MeetingEvent>(new EventReader<MeetingEvent>(this, $"Meeting-{meetingStarted.MeetingId}"), HandleMeetingEvent));
					return _meetingsHub.Clients.All.OnMeetingAddedToList(meetingStarted.MeetingId.ToString(), meetingStarted.Name);
				});

		private Task HandleMeetingEvent(MeetingEvent @event)
			=> @event.Match(
				itemAddedToAgenda => _meetingsHub.Clients.All.OnMeetingAgendaCountChanged(itemAddedToAgenda.MeetingId.ToString(), IncrementAgendaCount(itemAddedToAgenda.MeetingId))
			);

		private int IncrementAgendaCount(Guid meetingId)
			=> _agendaCounts[meetingId] = _agendaCounts.ContainsKey(meetingId) ? _agendaCounts[meetingId] + 1 : 1;

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