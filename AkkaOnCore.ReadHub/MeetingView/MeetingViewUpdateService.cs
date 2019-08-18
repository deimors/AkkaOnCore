using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkkaOnCore.Messages;
using AkkaOnCore.ReadModel.Meeting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AkkaOnCore.ReadHub.MeetingView
{
	public class MeetingViewUpdateService : UpdateService<MeetingViewEvent>
	{
		private readonly IHubContext<MeetingHub, IMeeting> _meetingHub;
		private readonly ILogger<MeetingViewUpdateService> _logger;

		private readonly IDictionary<Guid, MeetingViewReadModel> _readModels = new Dictionary<Guid, MeetingViewReadModel>();

		public MeetingViewUpdateService(IHubContext<MeetingHub, IMeeting> meetingHub, IEventStorage eventStorage, ILogger<MeetingViewUpdateService> logger)
			: base(eventStorage)
		{
			_meetingHub = meetingHub ?? throw new ArgumentNullException(nameof(meetingHub));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override void Initialize()
			=> AddProcessor<MeetingsEvent>("MeetingsActor", HandleMeetingsEvent);

		private IEnumerable<MeetingViewEvent> HandleMeetingsEvent(MeetingsEvent meetingsEvent)
		{
			_logger.LogInformation($"Received {meetingsEvent}");

			if (meetingsEvent is MeetingsEvent.MeetingStartedEvent meetingStarted)
			{
				_readModels.Add(meetingStarted.MeetingId, new MeetingViewReadModel(meetingStarted.MeetingId, meetingStarted.Name));

				AddProcessor<MeetingEvent>($"Meeting-{meetingStarted.MeetingId}", HandleMeetingEvent);
			}

			return Enumerable.Empty<MeetingViewEvent>();
		}

		private IEnumerable<MeetingViewEvent> HandleMeetingEvent(MeetingEvent meetingEvent)
		{
			_logger.LogInformation($"Received {meetingEvent}");

			return _readModels[meetingEvent.MeetingId].Integrate(meetingEvent);
		}

		protected override Task SendReadModelEvent(MeetingViewEvent @event)
		{
			_logger.LogInformation($"Sending Event {@event}");
			return Send(@event, _meetingHub.Clients.Group($"Meeting-{@event.MeetingId}"));
		}

		private static Task Send(MeetingViewEvent @event, IMeeting meeting)
			=> @event.Match(
				agendaItemAdded => meeting.OnAgendaItemAdded(agendaItemAdded.Description)
			);
	}
}