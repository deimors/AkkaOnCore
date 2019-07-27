using System;
using Akka.Actor;
using Akka.Persistence;

namespace AkkaOnCore.Actors
{
	public class MeetingActor : ReceivePersistentActor
	{
		private readonly Guid _id;
		private readonly string _name;
		public override string PersistenceId { get; }

		public MeetingActor(Guid id, string name)
		{
			_id = id;
			_name = name;

			PersistenceId = $"Meeting-{id}";
		}

		public static Props CreateProps(Guid id, string name)
			=> Props.Create(() => new MeetingActor(id, name));
	}
}