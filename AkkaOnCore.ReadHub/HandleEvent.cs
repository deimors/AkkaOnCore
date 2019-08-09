using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public delegate Task HandleEvent<TEvent>(TEvent @event);
}