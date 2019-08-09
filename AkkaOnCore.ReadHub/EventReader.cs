using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace AkkaOnCore.ReadHub
{
	public class EventReader<TEvent>
	{
		private const int ChunkCount = 128;

		private readonly IEventConnection _connection;
		private readonly string _persistenceId;

		private long _nextEventIndex = 0;

		public EventReader(IEventConnection connection, string persistenceId)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_persistenceId = persistenceId ?? throw new ArgumentNullException(nameof(persistenceId));
		}

		public async Task<IEnumerable<TEvent>> ReadNewestEvents()
		{
			var resolvedEvents = (await _connection.ReadEvents(_persistenceId, _nextEventIndex, ChunkCount)).ToArray();

			if (resolvedEvents.Any())
				_nextEventIndex = resolvedEvents.Last().Event.EventNumber + 1;

			return resolvedEvents.Select(DeserializeEvent);
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
}