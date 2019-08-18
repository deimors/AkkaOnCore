using AkkaOnCore.Messages;
using System;
using System.Collections.Generic;

namespace AkkaOnCore.ReadModel.Meeting
{
	public class MeetingViewReadModel
	{
		private readonly Dictionary<Guid, string> _agendaItems = new Dictionary<Guid, string>();

		public Guid Id { get; }
		public string Name { get; }
		public IReadOnlyDictionary<Guid, string> Agenda => _agendaItems;

		public MeetingViewReadModel(Guid id, string name)
		{
			Id = id;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public IEnumerable<MeetingViewEvent> Integrate(MeetingEvent meetingEvent)
			=> meetingEvent.Match(itemAddedToAgenda => AddToAgenda(itemAddedToAgenda.AgendaItemId, itemAddedToAgenda.Description));

		private IEnumerable<MeetingViewEvent> AddToAgenda(Guid agendaItemId, string description)
		{
			_agendaItems[agendaItemId] = description;

			return new MeetingViewEvent[] { new MeetingViewEvent.AgendaItemAdded(Id, description) };
		}
	}
}
