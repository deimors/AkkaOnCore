using Akka.Actor;
using AkkaOnCore.Domain;
using AkkaOnCore.Messages;
using System;

namespace AkkaOnCore.Actors
{
	public class MeetingActor : AggregateRootActor<MeetingEvent, MeetingCommand, MeetingCommandError>
	{
		public MeetingActor(Guid id, string name) : base(new MeetingAggregateRoot(id, name), $"Meeting-{id}")
		{
			Console.WriteLine($"Created '{name}' ({PersistenceId})");
		}

		public static Props CreateProps(Guid id, string name)
			=> Props.Create(() => new MeetingActor(id, name));
	}
}