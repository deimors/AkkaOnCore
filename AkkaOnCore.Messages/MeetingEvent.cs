using System;

namespace AkkaOnCore.Messages
{
	public abstract class MeetingEvent
	{
		public abstract TResult Match<TResult>(
			Func<ItemAddedToAgenda, TResult> itemAddedToAgenda
		);

		public abstract void Apply(
			Action<ItemAddedToAgenda> itemAddedToAgenda
		);

		public class ItemAddedToAgenda : MeetingEvent
		{
			public string Description { get; }
			public Guid MeetingId { get; }
			public Guid AgendaItemId { get; }

			public ItemAddedToAgenda(string description, Guid meetingId, Guid agendaItemId)
			{
				Description = description ?? throw new ArgumentNullException(nameof(description));
				MeetingId = meetingId;
				AgendaItemId = agendaItemId;
			}

			public override TResult Match<TResult>(Func<ItemAddedToAgenda, TResult> itemAddedToAgenda)
				=> itemAddedToAgenda(this);

			public override void Apply(Action<ItemAddedToAgenda> itemAddedToAgenda)
				=> itemAddedToAgenda(this);
		}
	}
}