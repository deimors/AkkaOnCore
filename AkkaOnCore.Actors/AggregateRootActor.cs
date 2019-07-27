using System.Collections.Generic;
using Akka.Actor;
using Akka.Persistence;
using Functional;

namespace AkkaOnCore.Actors
{
	public abstract class AggregateRootActor<TEvent, TCommand, TError> : ReceivePersistentActor
	{
		protected AggregateRootActor()
		{
			Recover<TEvent>(ApplyEvent);
			Command<TCommand>(RespondToCommand);
		}

		protected abstract void ApplyEvent(TEvent @event);
		protected abstract Result<IEnumerable<TEvent>, TError> HandleCommand(TCommand command);

		private void RespondToCommand(TCommand command)
			=> Sender.Tell(
				HandleCommand(command)
					.Do(events => PersistAll(events, ApplyEvent))
					.Select(_ => Unit.Value)
			);
	}
}