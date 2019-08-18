using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public interface IEventStorage
	{
		Task<EventSequence<TEvent>> ReadEvents<TEvent>(string persistenceId, long start, int count);
	}
}