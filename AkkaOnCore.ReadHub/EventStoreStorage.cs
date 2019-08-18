using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AkkaOnCore.ReadHub
{
	public class EventStoreStorage : IEventStorage
	{
		private readonly ILogger<EventStoreStorage> _logger;
		private readonly IEventStoreConnection _connection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@eventstore-node:1113; MaxReconnections=-1");

		private bool _isConnecting;
		private bool _isConnected;

		public EventStoreStorage(ILogger<EventStoreStorage> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_connection.Connected += OnConnected;
			_connection.Disconnected += OnDisconnected;
			_connection.Closed += OnClosed;
			_connection.ErrorOccurred += OnErrorOccurred;
		}

		public async Task<EventSequence<TEvent>> ReadEvents<TEvent>(string persistenceId, long start, int count)
		{
			if (!_isConnected && !_isConnecting)
			{
				_isConnecting = true;
				await _connection.ConnectAsync();
			}

			if (!_isConnected)
				return new EventSequence<TEvent>(start, start, Enumerable.Empty<TEvent>());

			var slice = await _connection.ReadStreamEventsForwardAsync(persistenceId, start, count, false);

			var events = slice.Events.Select(DeserializeEvent<TEvent>);

			return slice.Events.Any() 
				? new EventSequence<TEvent>(start, slice.LastEventNumber + 1, events) 
				: new EventSequence<TEvent>(start, start, Enumerable.Empty<TEvent>());
		}

		private static TEvent DeserializeEvent<TEvent>(ResolvedEvent resolvedEvent)
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
			_isConnecting = false;
		}
	}
}