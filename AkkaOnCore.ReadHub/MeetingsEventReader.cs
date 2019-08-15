using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AkkaOnCore.Messages;
using AkkaOnCore.ReadModel.Meetings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AkkaOnCore.ReadHub
{
	public class MeetingsEventReader : BackgroundService
	{
		private readonly IHubContext<MeetingsHub, IMeetings> _meetingsHub;
		private readonly IEventStorage _eventStorage;
		private readonly ILogger<MeetingsEventReader> _logger;

		private readonly List<IProcessEvents> _eventProcessors = new List<IProcessEvents>();

		private readonly MeetingsListReadModel _readModel = new MeetingsListReadModel();

		public MeetingsEventReader(IHubContext<MeetingsHub, IMeetings> meetingsHub, IEventStorage eventStorage, ILogger<MeetingsEventReader> logger)
		{
			_meetingsHub = meetingsHub ?? throw new ArgumentNullException(nameof(meetingsHub));
			_eventStorage = eventStorage ?? throw new ArgumentNullException(nameof(eventStorage));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override async Task ExecuteAsync(CancellationToken cancelToken)
		{
			try
			{
				await _eventStorage.Connect();

				AddStreamHandler<MeetingsEvent>("MeetingsActor", HandleMeetingsEvent);

				await ProcessLoop(cancelToken);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private async Task ProcessLoop(CancellationToken cancelToken)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				foreach (var processor in _eventProcessors.ToArray())
				{
					await processor.Process();
				}

				await Task.Delay(TimeSpan.FromMilliseconds(10), cancelToken);
			}
		}

		private Task HandleMeetingsEvent(MeetingsEvent meetingsEvent)
		{
			_logger.LogInformation($"Received {meetingsEvent}");

			if (meetingsEvent is MeetingsEvent.MeetingStartedEvent meetingStarted)
				AddStreamHandler<MeetingEvent>($"Meeting-{meetingStarted.MeetingId}", HandleMeetingEvent);

			return SendReadModelEvents(_readModel.Integrate(meetingsEvent));
		}

		private Task HandleMeetingEvent(MeetingEvent meetingEvent)
		{
			_logger.LogInformation($"Received {meetingEvent}");

			return SendReadModelEvents(_readModel.Integrate(meetingEvent));
		}

		private void AddStreamHandler<TEvent>(string persistenceId, HandleEvent<TEvent> handler)
			=> _eventProcessors.Add(new EventProcessor<TEvent>(new EventReader<TEvent>(_eventStorage, persistenceId), handler));

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

	}
}