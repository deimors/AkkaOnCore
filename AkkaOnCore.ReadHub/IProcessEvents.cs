using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public interface IProcessEvents
	{
		Task Process();
	}
}