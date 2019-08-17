using System;
using Akka.Actor;
using AkkaOnCore.Messages;
using Functional;
using Microsoft.Extensions.Logging;

namespace AkkaOnCore.QueryAPI.Meeting
{
	public class MeetingViewUpdateService : UpdateService
	{
		private readonly MeetingViewReadModelCollection _readModelCollection;
		private readonly ILogger<MeetingViewUpdateService> _logger;

		public MeetingViewUpdateService(ActorSystem actorSystem, MeetingViewReadModelCollection readModelCollection, ILogger<MeetingViewUpdateService> logger) 
			: base(actorSystem, logger)
		{
			_readModelCollection = readModelCollection ?? throw new ArgumentNullException(nameof(readModelCollection));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override void Initialize()
		{
			ListenToEvents<MeetingsEvent>("MeetingsActor", ApplyMeetingsEvent);
		}

		private void ApplyMeetingsEvent(MeetingsEvent @event)
		{
			@event.Apply(
				meetingStarted =>
				{
					_logger.LogInformation($"Adding Meeting {meetingStarted.MeetingId}");

					_readModelCollection.Add(meetingStarted.MeetingId, meetingStarted.Name);

					ListenToEvents<MeetingEvent>($"Meeting-{meetingStarted.MeetingId}", ApplyMeetingEvent);
				});
		}

		private void ApplyMeetingEvent(MeetingEvent @event)
		{
			_readModelCollection[@event.MeetingId].Apply(
				readModel => readModel.Integrate(@event), 
				() => _logger.LogError($"Can't apply meeting event, Meeting {@event.MeetingId} not found")
			);
		}
	}
}