using System;
using System.Collections.Generic;
using AkkaOnCore.Messages;
using Functional;

namespace AkkaOnCore.Domain
{
	public class MeetingsAggregateRoot : IAggregateRoot<MeetingsEvent, MeetingsCommand, MeetingsCommandError>
	{
		private readonly IDictionary<Guid, string> _meetings = new Dictionary<Guid, string>();

		public Result<IEnumerable<MeetingsEvent>, MeetingsCommandError> HandleCommand(MeetingsCommand command)
			=> command.Match(StartMeeting);

		private Result<IEnumerable<MeetingsEvent>, MeetingsCommandError> StartMeeting(MeetingsCommand.StartMeeting command)
			=> Result.Unit<MeetingsCommandError>()
				.FailIf(
					string.IsNullOrWhiteSpace(command.Name),
					() => new MeetingsCommandError("Meeting Name cannot be Empty or Whitespace")
				).FailIf(
					_meetings.Values.Contains(command.Name),
					() => new MeetingsCommandError($"Meeting with name '{command.Name}' already exists")
				).BuildSequence(
					new MeetingsEvent.MeetingStartedEvent(Guid.NewGuid(), command.Name) as MeetingsEvent
				);
				
		public void ApplyEvent(MeetingsEvent meetingsEvent)
			=> meetingsEvent.Apply(OnMeetingStarted);

		private void OnMeetingStarted(MeetingsEvent.MeetingStartedEvent e)
			=> _meetings.Add(e.MeetingId, e.Name);
	}
}
