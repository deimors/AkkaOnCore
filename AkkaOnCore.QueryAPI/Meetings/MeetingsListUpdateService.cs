using Akka.Actor;
using AkkaOnCore.Messages;
using AkkaOnCore.ReadModel.Meetings;
using Microsoft.Extensions.Logging;
using System;

namespace AkkaOnCore.QueryAPI.Meetings
{
	public class MeetingsListUpdateService : UpdateService
	{
		private readonly MeetingsListReadModel _readModel;

		public MeetingsListUpdateService(ActorSystem actorSystem, MeetingsListReadModel readModel, ILogger<MeetingsListUpdateService> logger) 
			: base(actorSystem, logger)
		{
			_readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
		}

		protected override void Initialize()
		{
			ListenToEvents<MeetingsEvent>("MeetingsActor", ApplyMeetingsEvent);
		}

		private void ApplyMeetingsEvent(MeetingsEvent @event)
		{
			_readModel.Integrate(@event);

			@event.Apply(
				meetingStarted => ListenToEvents<MeetingEvent>($"Meeting-{meetingStarted.MeetingId}", ApplyMeetingEvent)
			);
		}

		private void ApplyMeetingEvent(MeetingEvent @event)
		{
			_readModel.Integrate(@event);
		}
	}
}