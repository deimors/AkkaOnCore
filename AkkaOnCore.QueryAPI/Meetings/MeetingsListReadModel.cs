using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.EventStore.Query;
using Akka.Persistence.Query;
using Akka.Streams;
using AkkaOnCore.Messages;
using AkkaOnCore.QueryAPI.Messages;

namespace AkkaOnCore.QueryAPI.Meetings
{
	public class MeetingsListReadModel
	{
		private readonly List<MeetingListEntry> _meetings = new List<MeetingListEntry>();
		public IReadOnlyList<MeetingListEntry> Meetings => _meetings;

		public MeetingsListReadModel(ActorSystem actorSystem)
		{
			var readJournal = PersistenceQuery.Get(actorSystem).ReadJournalFor<EventStoreReadJournal>("akka.persistence.query.journal.eventstore");

			var source = readJournal.EventsByPersistenceId("MeetingsActor", 0, long.MaxValue);

			var materializer = ActorMaterializer.Create(actorSystem);

			source.RunForeach(envelope =>
			{
				switch (envelope.Event)
				{
					case MeetingsEvent.MeetingStartedEvent meetingStarted: 
						_meetings.Add(new MeetingListEntry { Name = meetingStarted.Name, Id = meetingStarted.MeetingId });
						break;
				}
			}, materializer);
		}
	}
}
