using System;

namespace AkkaOnCore.Messages
{
	public abstract class MeetingsCommand
	{
		public class StartMeeting : MeetingsCommand
		{
			public string Name { get; private set; }

			public StartMeeting(string name)
			{
				Name = name;
			}

			public override TResult Match<TResult>(Func<StartMeeting, TResult> startMeeting)
				=> startMeeting(this);

			public override void Apply(Action<StartMeeting> startMeeting)
				=> startMeeting(this);
		}

		public abstract TResult Match<TResult>(
			Func<StartMeeting, TResult> startMeeting
		);

		public abstract void Apply(
			Action<StartMeeting> startMeeting
		);
	}
}