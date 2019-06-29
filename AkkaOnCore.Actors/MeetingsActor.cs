using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Persistence;
using AkkaOnCore.Messages;

namespace AkkaOnCore.Actors
{
	public class MeetingsActor : ReceivePersistentActor
	{
		private readonly IDictionary<Guid, string> _meetings = new Dictionary<Guid, string>();

		public override string PersistenceId => "MeetingsActor";

		public MeetingsActor()
		{
			Recover<MeetingsEvent>(HandleMeetingsEvent);

			Command<StartMeetingCommand>(command => PersistAll(StartMeeting(command), HandleMeetingsEvent));

			Command<GetMeetingsQuery>(GetMeetings);
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());

		private IEnumerable<MeetingsEvent> StartMeeting(StartMeetingCommand command)
			=> Enumerable.Empty<MeetingsEvent>().Append(new MeetingStartedEvent(Guid.NewGuid(), command.Name));

		private void HandleMeetingsEvent(MeetingsEvent meetingsEvent)
		{
			switch (meetingsEvent)
			{
				case MeetingStartedEvent meetingStarted:
					OnMeetingStarted(meetingStarted);
					break;
			}
		}

		private void OnMeetingStarted(MeetingStartedEvent e)
		{
			_meetings.Add(e.MeetingId, e.Name);

			Sender.Tell(e.MeetingId);
		}

		private void GetMeetings(GetMeetingsQuery obj)
		{
			Sender.Tell(_meetings.ToList());
		}
	}
}
