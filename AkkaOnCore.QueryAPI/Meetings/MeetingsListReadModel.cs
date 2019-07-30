using AkkaOnCore.Messages;
using AkkaOnCore.QueryAPI.Messages;
using System;
using System.Collections.Generic;

namespace AkkaOnCore.QueryAPI.Meetings
{
	public class MeetingsListReadModel
	{
		private readonly IDictionary<Guid, MeetingListEntry> _meetings = new Dictionary<Guid, MeetingListEntry>();
		public IEnumerable<MeetingListEntry> Meetings => _meetings.Values;

		public void Integrate(MeetingsEvent @event)
			=> @event.Apply(
				meetingStarted => _meetings[meetingStarted.MeetingId] = new MeetingListEntry { Name = meetingStarted.Name, Id = meetingStarted.MeetingId }
			);

		public void Integrate(MeetingEvent @event)
			=> @event.Apply(
				itemAddedToAgenda => _meetings[itemAddedToAgenda.MeetingId].AgendaItemCount++
			);
	}
}
