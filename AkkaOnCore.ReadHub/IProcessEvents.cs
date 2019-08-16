using System.Collections.Generic;
using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public interface IProcessEvents<TReadModelEvents>
	{
		Task<IEnumerable<TReadModelEvents>> Process();
	}
}