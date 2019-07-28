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
		public MeetingsActor() : base(new MeetingsAggregateRoot(), "MeetingsActor")
		{
			Console.WriteLine("Created Meetings Actor");
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

			Console.WriteLine($"Created {actorRef.Path.ToString()}");
		}
	}
}
