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

		public MeetingAggregateRoot(Guid meetingId, string name)
		{
			_meetingId = meetingId;
			_name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public Result<IEnumerable<MeetingEvent>, MeetingCommandError> HandleCommand(MeetingCommand command)
			=> Result.Success<IEnumerable<MeetingEvent>, MeetingCommandError>(Enumerable.Empty<MeetingEvent>());

		public void ApplyEvent(MeetingEvent @event)
			=> @event.Apply();
	}
}