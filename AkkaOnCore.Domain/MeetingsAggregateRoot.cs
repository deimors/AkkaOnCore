using System;
using System.Collections.Generic;
using System.Linq;
using AkkaOnCore.Messages;
using Functional;

namespace AkkaOnCore.Domain
{
	public class MeetingsAggregateRoot
	{
		private readonly IDictionary<Guid, string> _meetings = new Dictionary<Guid, string>();

		public Result<IEnumerable<MeetingsEvent>, MeetingsCommandError> HandleCommand(MeetingsCommand command)
			=> command.Match(StartMeeting);

		private Result<IEnumerable<MeetingsEvent>, MeetingsCommandError> StartMeeting(MeetingsCommand.StartMeeting command)
			=> Result.Create(
				!_meetings.Values.Contains(command.Name), 
				() => new MeetingsEvent[] { new MeetingsEvent.MeetingStartedEvent(Guid.NewGuid(), command.Name) }.AsEnumerable(),
				() => new MeetingsCommandError($"Meeting with name '{command.Name}' already exists")
			);

		public void ApplyEvent(MeetingsEvent meetingsEvent)
			=> meetingsEvent.Apply(OnMeetingStarted);

		private void OnMeetingStarted(MeetingsEvent.MeetingStartedEvent e)
			=> _meetings.Add(e.MeetingId, e.Name);
	}
}
