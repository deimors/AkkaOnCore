using System;
using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub.MeetingView
{
	public interface IMeeting
	{
		Task OnAgendaItemAdded(Guid agendaItemId, string description);
	}
}