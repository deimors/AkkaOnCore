using System;

namespace AkkaOnCore.ReadModel.Meetings
{
	public class MeetingListEntry
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public int AgendaItemCount { get; set; }
	}
}