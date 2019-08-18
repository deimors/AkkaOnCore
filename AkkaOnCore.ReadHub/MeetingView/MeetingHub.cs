using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AkkaOnCore.ReadHub.MeetingView
{
	public class MeetingHub : Hub<IMeeting>
	{
		public Task Subscribe(string meetingId)
			=> Groups.AddToGroupAsync(Context.ConnectionId, $"Meeting-{meetingId}");
	}
}