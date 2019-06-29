using System;

namespace AkkaOnCore.Messages
{
	public class MeetingStartedEvent : MeetingsEvent
	{
		public Guid MeetingId { get; private set; }
		public string Name { get; private set; }

		public MeetingStartedEvent(Guid meetingId, string name)
		{
			MeetingId = meetingId;
			Name = name;
		}
	}
}