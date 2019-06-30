using Akka.Actor;
using Akka.Persistence;
using AkkaOnCore.Domain;
using AkkaOnCore.Messages;
using Functional;

namespace AkkaOnCore.Actors
{
	public class MeetingsActor : ReceivePersistentActor
	{
		private readonly MeetingsAggregateRoot _meetings = new MeetingsAggregateRoot();

		public override string PersistenceId => "MeetingsActor";

		public MeetingsActor()
		{
			Recover<MeetingsEvent>(_meetings.ApplyEvent);

			Command<MeetingsCommand>(
				command => Sender.Tell(
					_meetings
						.HandleCommand(command)
						.Do(events => PersistAll(events, _meetings.ApplyEvent))
						.Select(_ => Unit.Value)
					)
				);
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());
	}
}
