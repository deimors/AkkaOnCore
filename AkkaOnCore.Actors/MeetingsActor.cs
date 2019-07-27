using System;
using System.Collections.Generic;
using Akka.Actor;
using AkkaOnCore.Domain;
using AkkaOnCore.Messages;
using Functional;

namespace AkkaOnCore.Actors
{
	public class MeetingsActor : AggregateRootActor<MeetingsEvent, MeetingsCommand, MeetingsCommandError>
	{
		private readonly MeetingsAggregateRoot _meetings = new MeetingsAggregateRoot();

		public override string PersistenceId => "MeetingsActor";

		public MeetingsActor()
		{
			Console.WriteLine("Created Meetings Actor");
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());

		protected override Result<IEnumerable<MeetingsEvent>, MeetingsCommandError> HandleCommand(MeetingsCommand command)
			=> _meetings.HandleCommand(command);

		protected override void ApplyEvent(MeetingsEvent @event)
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
