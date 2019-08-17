using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.EventStore.Query;
using Akka.Persistence.Query;
using Akka.Streams;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AkkaOnCore.QueryAPI
{
	public abstract class UpdateService : BackgroundService
	{
		private readonly ActorSystem _actorSystem;
		private readonly ILogger _logger;

		protected UpdateService(ActorSystem actorSystem, ILogger logger)
		{
			_actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected abstract void Initialize();

		protected void ListenToEvents<TEvent>(string persistenceId, Action<TEvent> applyEvent)
			=> PersistenceQuery
				.Get(_actorSystem)
				.ReadJournalFor<EventStoreReadJournal>("akka.persistence.query.journal.eventstore")
				.EventsByPersistenceId(persistenceId, 0, long.MaxValue)
				.RunForeach(envelope =>
					{
						_logger.LogInformation($"Received {envelope.PersistenceId} ({envelope.SequenceNr}) :: {envelope.Event}");
						applyEvent((TEvent)envelope.Event);
					}, 
					ActorMaterializer.Create(_actorSystem)
				);

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Initialize();

			return Task.CompletedTask;
		}
	}
}