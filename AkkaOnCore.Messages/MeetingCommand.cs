using System;

namespace AkkaOnCore.Messages
{
	public abstract class MeetingCommand
	{
		public abstract TResult Match<TResult>(
			Func<AddToAgenda, TResult> addToAgenda
		);

		public abstract void Apply(
			Action<AddToAgenda> addToAgenda
		);

		public class AddToAgenda : MeetingCommand
		{
			public string Description { get; }

			public AddToAgenda(string description)
			{
				Description = description;
			}

			public override TResult Match<TResult>(Func<AddToAgenda, TResult> addToAgenda)
				=> addToAgenda(this);

			public override void Apply(Action<AddToAgenda> addToAgenda)
				=> addToAgenda(this);
		}
	}
}