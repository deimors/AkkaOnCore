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
		private readonly IDictionary<Guid, IActorRef> _meetingActorRefs = new Dictionary<Guid, IActorRef>();

		public MeetingsActor() : base(new MeetingsAggregateRoot(), "MeetingsActor")
		{
			Console.WriteLine("Created Meetings Actor");

			Command<Guid>(id => Sender.Tell(_meetingActorRefs[id]));
		}

		public static Props CreateProps()
			=> Props.Create(() => new MeetingsActor());

		protected override void OnEventApplied(MeetingsEvent @event)
			=> @event.Apply(
				meetingStarted => CreateMeetingActor(meetingStarted.MeetingId, meetingStarted.Name)
			);

		private void CreateMeetingActor(Guid id, string name)
		{
			var actorRef = Context.ActorOf(MeetingActor.CreateProps(id, name), id.ToString());

			_meetingActorRefs[id] = actorRef;

			Console.WriteLine($"Created {actorRef.Path.ToString()}");
		}
	}
}
