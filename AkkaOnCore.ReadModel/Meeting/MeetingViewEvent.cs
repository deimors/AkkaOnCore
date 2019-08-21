using System;

namespace AkkaOnCore.ReadModel.Meeting
{
	public abstract class MeetingViewEvent
	{
		public abstract Guid MeetingId { get; }

		public abstract TResult Match<TResult>(
			Func<AgendaItemAdded, TResult> agendaItemAdded
		);

		public abstract void Apply(
			Action<AgendaItemAdded> agendaItemAdded
		);

		public class AgendaItemAdded : MeetingViewEvent
		{
			public override Guid MeetingId { get; }
			public Guid AgendaItemId { get; }
			public string Description { get; }

			public AgendaItemAdded(Guid agendaItemId, Guid meetingId, string description)
			{
				AgendaItemId = agendaItemId;
				MeetingId = meetingId;
				Description = description ?? throw new ArgumentNullException(nameof(description));
			}

			public override TResult Match<TResult>(Func<AgendaItemAdded, TResult> agendaItemAdded)
				=> agendaItemAdded(this);

			public override void Apply(Action<AgendaItemAdded> agendaItemAdded)
				=> agendaItemAdded(this);
		}
	}
}