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
			=> Enumerable.Empty<MeetingsEvent>().Append(new MeetingStartedEvent(Guid.NewGuid(), command.Name));

		public void HandleMeetingsEvent(MeetingsEvent meetingsEvent)
		{
			switch (meetingsEvent)
			{
				case MeetingStartedEvent meetingStarted:
					OnMeetingStarted(meetingStarted);
					break;
			}
		}

		private void OnMeetingStarted(MeetingStartedEvent e)
		{
			_meetings.Add(e.MeetingId, e.Name);
		}
	}
}
