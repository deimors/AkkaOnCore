using System;

namespace AkkaOnCore.Messages
{
	public abstract class MeetingsEvent
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

			public override TResult Match<TResult>(
				Func<MeetingStartedEvent, TResult> meetingStarted
			) => meetingStarted(this);

			public override void Apply(
				Action<MeetingStartedEvent> meetingStarted
			) => meetingStarted(this);
		}

		public abstract TResult Match<TResult>(
			Func<MeetingStartedEvent, TResult> meetingStarted
		);

		public abstract void Apply(
			Action<MeetingStartedEvent> meetingStarted
		);
	}
}