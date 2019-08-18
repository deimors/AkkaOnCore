using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub.MeetingsList
{
	public interface IMeetings
	{
		Task OnMeetingAddedToList(string meetingId, string name);
		Task OnMeetingAgendaCountChanged(string meetingId, long newCount);
	}
}