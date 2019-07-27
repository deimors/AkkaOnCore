using System;
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
			Console.WriteLine("Creating Meetings Actor");

			Recover<MeetingsEvent>(ApplyEvent);

			Command<MeetingsCommand>(HandleCommand);
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());

		private void HandleCommand(MeetingsCommand command)
			=> Sender.Tell(
				_meetings
					.HandleCommand(command)
					.Do(events => PersistAll(events, ApplyEvent))
					.Select(_ => Unit.Value)
			);

		private void ApplyEvent(MeetingsEvent @event)
		{
			_meetings.ApplyEvent(@event);

			@event.Apply(
				meetingStarted => CreateMeetingActor(meetingStarted.MeetingId, meetingStarted.Name)
			);
		}

		private void CreateMeetingActor(Guid id, string name)
		{
			var actorRef = Context.ActorOf(MeetingActor.CreateProps(id, name), id.ToString());

			Console.WriteLine($"Created {actorRef.Path.ToString()}");
		}
	}
}
