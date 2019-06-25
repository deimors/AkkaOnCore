using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Persistence;

namespace AkkaOnCore.Actors
{
	public class MeetingsActor : ReceivePersistentActor
	{
		private readonly IDictionary<Guid, string> _meetings = new Dictionary<Guid, string>();

		public override string PersistenceId => "MeetingsActor";

		public MeetingsActor()
		{
			Recover<StartMeetingCommand>(StartMeeting);
			Command<StartMeetingCommand>(command => Persist(command, StartMeeting));

			Command<GetMeetingsQuery>(GetMeetings);
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());

		private void StartMeeting(StartMeetingCommand command)
		{
			var newMeetingId = Guid.NewGuid();

			_meetings.Add(newMeetingId, command.Name);

			Sender.Tell(newMeetingId);
		}

		private void GetMeetings(GetMeetingsQuery obj)
		{
			Sender.Tell(_meetings.ToList());
		}
	}
}
