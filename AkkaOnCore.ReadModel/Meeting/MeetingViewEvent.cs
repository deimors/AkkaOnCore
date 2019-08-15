using System;

namespace AkkaOnCore.ReadModel.Meeting
{
	public abstract class MeetingViewEvent
	{
		public abstract TResult Match<TResult>(
			Func<AgendaItemAdded, TResult> agendaItemAdded
		);

		public abstract void Apply(
			Action<AgendaItemAdded> agendaItemAdded
		);

		public class AgendaItemAdded : MeetingViewEvent
		{
			private readonly string _description;

			public AgendaItemAdded(string description)
			{
				_description = description;
			}

			public override TResult Match<TResult>(Func<AgendaItemAdded, TResult> agendaItemAdded)
				=> agendaItemAdded(this);

			public override void Apply(Action<AgendaItemAdded> agendaItemAdded)
				=> agendaItemAdded(this);
		}
	}
}