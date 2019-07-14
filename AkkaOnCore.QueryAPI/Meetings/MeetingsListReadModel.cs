using System.Collections.Generic;
using System.Linq;
using AkkaOnCore.Messages;
using AkkaOnCore.QueryAPI.Messages;

namespace AkkaOnCore.QueryAPI.Meetings
{
	public class MeetingsListReadModel
	{
		private readonly List<MeetingListEntry> _meetings = new List<MeetingListEntry>();
		public IReadOnlyList<MeetingListEntry> Meetings => _meetings;

		public void Integrate(MeetingsEvent meetingsEvent)
		{
			switch (meetingsEvent)
			{
				case MeetingsEvent.MeetingStartedEvent meetingStarted:
					_meetings.Add(new MeetingListEntry {Name = meetingStarted.Name, Id = meetingStarted.MeetingId});
					break;
			}
		}
	}
}
