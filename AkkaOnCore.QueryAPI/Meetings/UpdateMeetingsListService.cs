using System;
using System.Threading;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Persistence.EventStore.Query;
using Akka.Persistence.Query;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaOnCore.Messages;
using AkkaOnCore.ReadModel.Meetings;
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
			ListenToEvents<MeetingsEvent>("MeetingsActor", ApplyMeetingsEvent);

			return Task.CompletedTask;
		}

		private void ListenToEvents<TEvent>(string persistenceId, Action<TEvent> applyEvent)
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

		private void ApplyMeetingsEvent(MeetingsEvent @event)
		{
			_readModel.Integrate(@event);

			@event.Apply(
				meetingStarted => ListenToEvents<MeetingEvent>($"Meeting-{meetingStarted.MeetingId}", ApplyMeetingEvent)
			);
		}

		private void ApplyMeetingEvent(MeetingEvent @event)
		{
			_readModel.Integrate(@event);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}