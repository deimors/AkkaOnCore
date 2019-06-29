using System;
using System.Collections.Generic;
using System.Linq;
using AkkaOnCore.Messages;

namespace AkkaOnCore.Domain
{
	public class MeetingsAggregateRoot
	{
		private readonly IDictionary<Guid, string> _meetings = new Dictionary<Guid, string>();

		public IEnumerable<MeetingsEvent> HandleCommand(MeetingsCommand command)
			=> command.Match(StartMeeting);

		private IEnumerable<MeetingsEvent> StartMeeting(MeetingsCommand.StartMeeting command)
		{
			yield return new MeetingsEvent.MeetingStartedEvent(Guid.NewGuid(), command.Name);
		}

		public void ApplyEvent(MeetingsEvent meetingsEvent)
			=> meetingsEvent.Apply(OnMeetingStarted);

		private void OnMeetingStarted(MeetingsEvent.MeetingStartedEvent e)
			=> _meetings.Add(e.MeetingId, e.Name);
	}
}
