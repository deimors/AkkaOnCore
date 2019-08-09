using System;
using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public class EventProcessor<TEvent> : IProcessEvents
	{
		private readonly EventReader<TEvent> _reader;
		private readonly HandleEvent<TEvent> _handleEvent;

		public EventProcessor(EventReader<TEvent> reader, HandleEvent<TEvent> handleEvent)
		{
			_reader = reader ?? throw new ArgumentNullException(nameof(reader));
			_handleEvent = handleEvent ?? throw new ArgumentNullException(nameof(handleEvent));
		}

		public async Task Process()
		{
			var events = await _reader.ReadNewestEvents();

			foreach (var @event in events)
			{
				await _handleEvent(@event);
			}
		}
	}
}