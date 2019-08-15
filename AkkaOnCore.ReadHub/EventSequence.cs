using System.Collections.Generic;

namespace AkkaOnCore.ReadHub
{
	public struct EventSequence<TEvent>
	{
		public long Start { get; }
		public long NextToRead { get; }
		public IEnumerable<TEvent> Events { get; }

		public EventSequence(long start, long nextToRead, IEnumerable<TEvent> events)
		{
			Start = start;
			NextToRead = nextToRead;
			Events = events;
		}
	}
}