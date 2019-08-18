using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AkkaOnCore.Messages;

namespace AkkaOnCore.ReadModel.Meeting
{
	public class MeetingViewReadModel
	{
		private readonly List<string> _agendaItems = new List<string>();

		public MeetingViewReadModel(Guid id, string name)
		{
			Id = id;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public Guid Id { get; }
		public string Name { get; }

		public IEnumerable<string> Agenda => _agendaItems;

		public IEnumerable<MeetingViewEvent> Integrate(MeetingEvent meetingEvent)
			=> meetingEvent.Match(itemAddedToAgenda => AddToAgenda(itemAddedToAgenda.Description));

		private IEnumerable<MeetingViewEvent> AddToAgenda(string description)
		{
			_agendaItems.Add(description);

			return new MeetingViewEvent[] { new MeetingViewEvent.AgendaItemAdded(Id, description) };
		}
	}
}
