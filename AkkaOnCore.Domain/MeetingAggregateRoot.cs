using System;
using System.Collections.Generic;
using System.Linq;
using AkkaOnCore.Messages;
using Functional;

namespace AkkaOnCore.Domain
{
	public class MeetingAggregateRoot : IAggregateRoot<MeetingEvent, MeetingCommand, MeetingCommandError>
	{
		private readonly Guid _meetingId;
		private readonly string _name;

		private readonly IDictionary<Guid, string> _agendaItems = new Dictionary<Guid, string>();

		public MeetingAggregateRoot(Guid meetingId, string name)
		{
			_meetingId = meetingId;
			_name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public Result<IEnumerable<MeetingEvent>, MeetingCommandError> HandleCommand(MeetingCommand command)
			=> command.Match(AddToAgenda);

		private Result<IEnumerable<MeetingEvent>, MeetingCommandError> AddToAgenda(MeetingCommand.AddToAgenda command)
			=> Result.Unit<MeetingCommandError>()
				.BuildSequence((MeetingEvent)new MeetingEvent.ItemAddedToAgenda(command.Description, _meetingId, Guid.NewGuid()));

		public void ApplyEvent(MeetingEvent @event)
			=> @event.Apply(OnItemAddedToAgenda);

		private void OnItemAddedToAgenda(MeetingEvent.ItemAddedToAgenda @event)
			=> _agendaItems[@event.AgendaItemId] = @event.Description;
	}
}