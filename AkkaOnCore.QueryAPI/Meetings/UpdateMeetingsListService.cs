using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.EventStore.Query;
using Akka.Persistence.Query;
using Akka.Streams;
using AkkaOnCore.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AkkaOnCore.QueryAPI.Meetings
{
	public class UpdateMeetingsListService : IHostedService
	{
		private readonly ActorSystem _actorSystem;
		private readonly MeetingsListReadModel _readModel;
		private readonly ILogger<UpdateMeetingsListService> _logger;

		public UpdateMeetingsListService(ActorSystem actorSystem, MeetingsListReadModel readModel, ILogger<UpdateMeetingsListService> logger)
		{
			_actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
			_readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var readJournal = PersistenceQuery.Get(_actorSystem).ReadJournalFor<EventStoreReadJournal>("akka.persistence.query.journal.eventstore");

			var source = readJournal.EventsByPersistenceId("MeetingsActor", 0, long.MaxValue);

			var materializer = ActorMaterializer.Create(_actorSystem);

			source.RunForeach(envelope => ApplyEvent(envelope), materializer);

			return Task.CompletedTask;
		}

		private void ApplyEvent(EventEnvelope envelope)
		{
			_logger.LogInformation($"Received {envelope.SequenceNr} :: {envelope.PersistenceId}");
			_readModel.Integrate((MeetingsEvent)envelope.Event);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}