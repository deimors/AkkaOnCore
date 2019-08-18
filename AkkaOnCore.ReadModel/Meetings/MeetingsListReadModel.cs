using System;
using System.Collections.Generic;
using AkkaOnCore.Messages;

namespace AkkaOnCore.ReadModel.Meetings
{
	public class MeetingsListReadModel
	{
		private readonly Dictionary<Guid, MeetingListEntry> _meetings = new Dictionary<Guid, MeetingListEntry>();

		public IReadOnlyDictionary<Guid, MeetingListEntry> Meetings => _meetings;

		public IEnumerable<MeetingsListEvent> Integrate(MeetingsEvent @event)
			=> @event.Match(
				startedEvent => AddMeeting(startedEvent.MeetingId, startedEvent.Name)
			);

		private IEnumerable<MeetingsListEvent> AddMeeting(Guid meetingId, string name)
		{
			_meetings[meetingId] = new MeetingListEntry { Name = name, Id = meetingId };

			return new MeetingsListEvent[] { new MeetingsListEvent.MeetingAdded(meetingId.ToString(), name) };
		}

		public IEnumerable<MeetingsListEvent> Integrate(MeetingEvent @event)
			=> @event.Match(
				itemAddedToAgenda => IncrementAgendaItemCount(itemAddedToAgenda.MeetingId)
			);

		private IEnumerable<MeetingsListEvent> IncrementAgendaItemCount(Guid meetingId)
		{
			_meetings[meetingId].AgendaItemCount++;

			return new MeetingsListEvent[] { new MeetingsListEvent.AgendaItemCountChanged(meetingId.ToString(), _meetings[meetingId].AgendaItemCount) };
		}
	}
}
