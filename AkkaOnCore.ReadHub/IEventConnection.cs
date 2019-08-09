using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace AkkaOnCore.ReadHub
{
	public interface IEventConnection
	{
		Task<IEnumerable<ResolvedEvent>> ReadEvents(string id, long start, int count);
	}
}