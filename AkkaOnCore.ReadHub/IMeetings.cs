using System;
using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public interface IMeetings
	{
		Task OnMeetingAddedToList(string meetingId, string name);
	}
}