using System;
using System.Collections.Generic;
using System.Linq;
using AkkaOnCore.Messages;

namespace AkkaOnCore.Domain
{
	public class MeetingsAggregateRoot
	{
		private readonly IDictionary<Guid, string> _meetings = new Dictionary<Guid, string>();

		public IEnumerable<MeetingsEvent> StartMeeting(StartMeetingCommand command)
		{
			yield return new MeetingsEvent.MeetingStartedEvent(Guid.NewGuid(), command.Name);
		}

		public void HandleMeetingsEvent(MeetingsEvent meetingsEvent)
			=> meetingsEvent.Apply(OnMeetingStarted);

		private void OnMeetingStarted(MeetingsEvent.MeetingStartedEvent e)
			=> _meetings.Add(e.MeetingId, e.Name);
	}
}
