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
			Recover<MeetingsEvent>(_meetings.ApplyEvent);

			Command<MeetingsCommand>(command => PersistAll(_meetings.HandleCommand(command), _meetings.ApplyEvent));
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());
	}
}
