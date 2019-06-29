using Akka.Actor;
using Akka.Persistence;
using AkkaOnCore.Domain;
using AkkaOnCore.Messages;

namespace AkkaOnCore.Actors
{
	public class MeetingsActor : ReceivePersistentActor
	{
		private readonly MeetingsAggregateRoot _meetings = new MeetingsAggregateRoot();

		public override string PersistenceId => "MeetingsActor";

		public MeetingsActor()
		{
			Recover<MeetingsEvent>(_meetings.HandleMeetingsEvent);

			Command<StartMeetingCommand>(command => PersistAll(_meetings.StartMeeting(command), _meetings.HandleMeetingsEvent));
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());
	}
}
