using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AkkaOnCore.Messages;
using AkkaOnCore.ReadModel.Meetings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AkkaOnCore.ReadHub.MeetingsList
{
	public class MeetingsListUpdateService : UpdateService<MeetingsListEvent>
	{
		private readonly IHubContext<MeetingsHub, IMeetings> _meetingsHub;
		private readonly ILogger<MeetingsListUpdateService> _logger;

		private readonly MeetingsListReadModel _readModel = new MeetingsListReadModel();

		public MeetingsListUpdateService(IHubContext<MeetingsHub, IMeetings> meetingsHub, IEventStorage eventStorage, ILogger<MeetingsListUpdateService> logger)
			: base(eventStorage)
		{
			_meetingsHub = meetingsHub ?? throw new ArgumentNullException(nameof(meetingsHub));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override void Initialize()
			=> AddProcessor<MeetingsEvent>("MeetingsActor", HandleMeetingsEvent);

		private IEnumerable<MeetingsListEvent> HandleMeetingsEvent(MeetingsEvent meetingsEvent)
		{
			_logger.LogInformation($"Received {meetingsEvent}");

			if (meetingsEvent is MeetingsEvent.MeetingStartedEvent meetingStarted)
				AddProcessor<MeetingEvent>($"Meeting-{meetingStarted.MeetingId}", HandleMeetingEvent);

			return _readModel.Integrate(meetingsEvent);
		}

		private IEnumerable<MeetingsListEvent> HandleMeetingEvent(MeetingEvent meetingEvent)
		{
			_logger.LogInformation($"Received {meetingEvent}");

			return _readModel.Integrate(meetingEvent);
		}

		protected override Task SendReadModelEvent(MeetingsListEvent @event)
			=> @event.Match(
				meetingAdded => _meetingsHub.Clients.All.OnMeetingAddedToList(meetingAdded.MeetingId, meetingAdded.Name),
				agendaItemCountChanged => _meetingsHub.Clients.All.OnMeetingAgendaCountChanged(agendaItemCountChanged.MeetingId, agendaItemCountChanged.NewCount)
			);

	}
}