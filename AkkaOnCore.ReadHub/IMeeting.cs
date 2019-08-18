using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public interface IMeeting
	{
		Task OnAgendaItemAdded(string description);
	}
}