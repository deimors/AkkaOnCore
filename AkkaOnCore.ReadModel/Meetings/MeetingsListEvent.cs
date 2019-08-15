using System;

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
}