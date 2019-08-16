using System.Collections.Generic;
using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public delegate IEnumerable<TReadModelEvent> HandleEvent<TEvent, TReadModelEvent>(TEvent @event);
}