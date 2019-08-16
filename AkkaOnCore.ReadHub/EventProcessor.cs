using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AkkaOnCore.ReadHub
{
	public class EventProcessor<TEvent, TReadModelEvent> : IProcessEvents<TReadModelEvent>
	{
		private readonly EventReader<TEvent> _reader;
		private readonly HandleEvent<TEvent, TReadModelEvent> _handleEvent;

		public EventProcessor(EventReader<TEvent> reader, HandleEvent<TEvent, TReadModelEvent> handleEvent)
		{
			_reader = reader ?? throw new ArgumentNullException(nameof(reader));
			_handleEvent = handleEvent ?? throw new ArgumentNullException(nameof(handleEvent));
		}

		public async Task<IEnumerable<TReadModelEvent>> Process()
			=> (await _reader.ReadNewestEvents()).SelectMany(_handleEvent.Invoke);
	}
}