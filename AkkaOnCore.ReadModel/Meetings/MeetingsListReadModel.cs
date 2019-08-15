using System;
using System.Collections.Generic;
using AkkaOnCore.Messages;

namespace AkkaOnCore.ReadModel.Meetings
{
	public abstract class MeetingsListEvent
	{
		public abstract TResult Match<TResult>(
			Func<MeetingAdded, TResult> meetingAdded,
			Func<AgendaItemCountChanged, TResult> agendaItemCountChanged
		);

		public abstract void Apply(
			Action<MeetingAdded> meetingAdded,
			Action<AgendaItemCountChanged> agendaItemCountChanged
		);

		public class MeetingAdded : MeetingsListEvent
		{
			public string MeetingId { get; }
			public string Name { get; }

			public MeetingAdded(string meetingId, string name)
			{
				MeetingId = meetingId;
				Name = name;
			}

			public override TResult Match<TResult>(
				Func<MeetingAdded, TResult> meetingAdded,
				Func<AgendaItemCountChanged, TResult> agendaItemCountChanged
			) => meetingAdded(this);

			public override void Apply(
				Action<MeetingAdded> meetingAdded,
				Action<AgendaItemCountChanged> agendaItemCountChanged
			) => meetingAdded(this);
		}

		public class AgendaItemCountChanged : MeetingsListEvent
		{
			public string MeetingId { get; }
			public long NewCount { get; }

			public AgendaItemCountChanged(string meetingId, long newCount)
			{
				MeetingId = meetingId;
				NewCount = newCount;
			}

			public override TResult Match<TResult>(
				Func<MeetingAdded, TResult> meetingAdded,
				Func<AgendaItemCountChanged, TResult> agendaItemCountChanged
			) => agendaItemCountChanged(this);

			public override void Apply(
				Action<MeetingAdded> meetingAdded,
				Action<AgendaItemCountChanged> agendaItemCountChanged
			) => agendaItemCountChanged(this);
		}
	}
	public class MeetingsListReadModel
	{
		private readonly IDictionary<Guid, MeetingListEntry> _meetings = new Dictionary<Guid, MeetingListEntry>();
		public IEnumerable<MeetingListEntry> Meetings => _meetings.Values;

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
