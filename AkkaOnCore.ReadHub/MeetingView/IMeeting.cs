using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub.MeetingView
{
	public interface IMeeting
	{
		Task OnAgendaItemAdded(string description);
	}
}