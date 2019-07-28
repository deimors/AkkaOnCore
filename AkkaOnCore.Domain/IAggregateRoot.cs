using System.Collections.Generic;
using Functional;

namespace AkkaOnCore.Domain
{
	public interface IAggregateRoot<TEvent, TCommand, TError>
	{
		Result<IEnumerable<TEvent>, TError> HandleCommand(TCommand command);
		void ApplyEvent(TEvent @event);
	}
}