using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public class EventReader<TEvent>
	{
		private const int ChunkCount = 128;

		private readonly IEventStorage _storage;
		private readonly string _persistenceId;

		private long _nextToRead = 0;

		public EventReader(IEventStorage storage, string persistenceId)
		{
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
			_persistenceId = persistenceId ?? throw new ArgumentNullException(nameof(persistenceId));
		}

		public async Task<IEnumerable<TEvent>> ReadNewestEvents()
		{
			var sequence = await _storage.ReadEvents<TEvent>(_persistenceId, _nextToRead, ChunkCount);

			_nextToRead = sequence.NextToRead;

			return sequence.Events;
		}
	}
}