using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.EventStore.Query;
using Akka.Persistence.Query;
using Akka.Streams;
using AkkaOnCore.Messages;
using Microsoft.Extensions.Hosting;

namespace AkkaOnCore.QueryAPI.Meetings
{
	public class UpdateMeetingsListService : IHostedService
	{
		private readonly ActorSystem _actorSystem;
		private readonly MeetingsListReadModel _readModel;

		public UpdateMeetingsListService(ActorSystem actorSystem, MeetingsListReadModel readModel)
		{
			_actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
			_readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var readJournal = PersistenceQuery.Get(_actorSystem).ReadJournalFor<EventStoreReadJournal>("akka.persistence.query.journal.eventstore");

			var source = readJournal.EventsByPersistenceId("MeetingsActor", 0, long.MaxValue);

			var materializer = ActorMaterializer.Create(_actorSystem);

			source.RunForeach(envelope => _readModel.Integrate((MeetingsEvent)envelope.Event), materializer);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}