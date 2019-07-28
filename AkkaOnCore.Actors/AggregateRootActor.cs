using Akka.Actor;
using Akka.Persistence;
using AkkaOnCore.Domain;
using Functional;
using System;

namespace AkkaOnCore.Actors
{
	public abstract class AggregateRootActor<TEvent, TCommand, TError> : ReceivePersistentActor
	{
		private readonly IAggregateRoot<TEvent, TCommand, TError> _root;

		protected AggregateRootActor(IAggregateRoot<TEvent, TCommand, TError> root, string persistenceId)
		{
			_root = root ?? throw new ArgumentNullException(nameof(root));
			PersistenceId = persistenceId ?? throw new ArgumentNullException(nameof(persistenceId));

			Recover<TEvent>(ApplyEvent);
			Command<TCommand>(HandleCommand);
		}

		public sealed override string PersistenceId { get; }

		protected virtual void OnEventApplied(TEvent @event) { }

		private void ApplyEvent(TEvent @event)
		{
			_root.ApplyEvent(@event);
			OnEventApplied(@event);
		}

		private void HandleCommand(TCommand command)
			=> Sender.Tell(
				_root
					.HandleCommand(command)
					.Do(events => PersistAll(events, ApplyEvent))
					.Select(_ => Unit.Value)
			);
	}
}