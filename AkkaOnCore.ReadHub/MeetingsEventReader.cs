using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkkaOnCore.Messages;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AkkaOnCore.ReadHub
{
	public delegate Task<IEnumerable<ResolvedEvent>> GetEventsSlice(string persistenceId, long startIndex, int count);

	public delegate Task HandleEvent<TEvent>(TEvent @event);

	public interface IProcessEvents
	{
		Task Process();
	}

	public class EventProcessor<TEvent> : IProcessEvents
	{
		private const int ChunkCount = 128;

		private readonly string _persistenceId;
		private readonly GetEventsSlice _getEventsSlice;
		private readonly HandleEvent<TEvent> _handleEvent;

		private long _nextEventIndex;

		public EventProcessor(string persistenceId, GetEventsSlice getEventsSlice, HandleEvent<TEvent> handleEvent)
		{
			_persistenceId = persistenceId ?? throw new ArgumentNullException(nameof(persistenceId));
			_getEventsSlice = getEventsSlice ?? throw new ArgumentNullException(nameof(getEventsSlice));
			_handleEvent = handleEvent ?? throw new ArgumentNullException(nameof(handleEvent));
		}

		public async Task Process()
		{
			var slice = await _getEventsSlice(_persistenceId, _nextEventIndex, ChunkCount);

			_nextEventIndex = await ProcessEvents(slice);
		}

		private async Task<long> ProcessEvents(IEnumerable<ResolvedEvent> events)
		{
			var nextEventIndex = _nextEventIndex;

			foreach (var resolvedEvent in events)
			{
				Console.WriteLine($"Received {resolvedEvent.Event.EventType} ({resolvedEvent.Event.EventId})");

				var @event = DeserializeEvent(resolvedEvent);

				await _handleEvent(@event);

				nextEventIndex = resolvedEvent.Event.EventNumber + 1;
			}

			return nextEventIndex;
		}

		private static TEvent DeserializeEvent(ResolvedEvent resolvedEvent)
		{
			dynamic metadata;

			using (var stream = new MemoryStream(resolvedEvent.Event.Metadata))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			using (var jsonReader = new JsonTextReader(reader))
			{
				metadata = JsonSerializer.Create().Deserialize(jsonReader);
			}

			using (var stream = new MemoryStream(resolvedEvent.Event.Data))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				var eventType = Type.GetType((string)metadata.clrEventType);
				var deserialized = JsonSerializer.Create().Deserialize(reader, eventType);
				return (TEvent)deserialized;
			}
		}
	}
	public class MeetingsEventReader : BackgroundService
	{
		private readonly IHubContext<MeetingsHub, IMeetings> _meetingsHub;
		private readonly ILogger<MeetingsEventReader> _logger;

		private bool _isConnected;
		private readonly IEventStoreConnection _connection;

		private readonly List<IProcessEvents> _eventProcessors = new List<IProcessEvents>();

		private readonly Dictionary<Guid, int> _agendaCounts = new Dictionary<Guid, int>();

		public MeetingsEventReader(IHubContext<MeetingsHub, IMeetings> meetingsHub, ILogger<MeetingsEventReader> logger)
		{
			_meetingsHub = meetingsHub;
			_logger = logger;
			_connection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@eventstore-node:1113; MaxReconnections=-1");
		}

		protected override async Task ExecuteAsync(CancellationToken cancelToken)
		{
			try
			{
				_connection.Connected += OnConnected;
				_connection.Disconnected += OnDisconnected;
				_connection.Closed += OnClosed;
				_connection.ErrorOccurred += OnErrorOccurred;

				await _connection.ConnectAsync();

				_logger.LogDebug("After ConnectAsync()");

				_eventProcessors.Add(new EventProcessor<MeetingsEvent>("MeetingsActor", ReadEvents, HandleMeetingsEvent));

				while (!cancelToken.IsCancellationRequested)
				{
					foreach (var processor in _eventProcessors.ToArray())
					{
						await processor.Process();
					}

					await Task.Delay(TimeSpan.FromMilliseconds(10), cancelToken);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private async Task<IEnumerable<ResolvedEvent>> ReadEvents(string id, long start, int count)
			=> _isConnected 
				? (await _connection.ReadStreamEventsForwardAsync(id, start, count, false)).Events.AsEnumerable()
				: Enumerable.Empty<ResolvedEvent>();

		private Task HandleMeetingsEvent(MeetingsEvent meetingsEvent)
			=> meetingsEvent.Match(
				meetingStarted =>
				{
					_eventProcessors.Add(new EventProcessor<MeetingEvent>($"Meeting-{meetingStarted.MeetingId}", ReadEvents, HandleMeetingEvent));
					return _meetingsHub.Clients.All.OnMeetingAddedToList(meetingStarted.MeetingId.ToString(), meetingStarted.Name);
				});

		private Task HandleMeetingEvent(MeetingEvent @event)
			=> @event.Match(
				itemAddedToAgenda => _meetingsHub.Clients.All.OnMeetingAgendaCountChanged(itemAddedToAgenda.MeetingId.ToString(), IncrementAgendaCount(itemAddedToAgenda.MeetingId))
			);

		private int IncrementAgendaCount(Guid meetingId)
			=> _agendaCounts[meetingId] = _agendaCounts.ContainsKey(meetingId) ? _agendaCounts[meetingId] + 1 : 1;

		private void OnErrorOccurred(object sender, ClientErrorEventArgs e)
		{
			_logger.LogError(e.Exception.ToString());
		}

		private void OnClosed(object sender, ClientClosedEventArgs e)
		{
			_logger.LogInformation("Closed");
		}

		private void OnDisconnected(object sender, ClientConnectionEventArgs e)
		{
			_logger.LogInformation("Disconnected");
			_isConnected = false;
		}

		private void OnConnected(object sender, ClientConnectionEventArgs e)
		{
			_logger.LogInformation("Connected");
			_isConnected = true;
		}
	}
}