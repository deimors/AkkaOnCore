using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace AkkaOnCore.ReadHub
{
	public abstract class UpdateService<TReadModelEvent> : BackgroundService
	{
		private readonly IEventStorage _eventStorage;
		private readonly List<IProcessEvents<TReadModelEvent>> _eventProcessors = new List<IProcessEvents<TReadModelEvent>>();

		protected UpdateService(IEventStorage eventStorage)
		{
			_eventStorage = eventStorage ?? throw new ArgumentNullException(nameof(eventStorage));
		}

		protected abstract void Initialize();

		protected abstract Task SendReadModelEvent(TReadModelEvent @event);

		protected override async Task ExecuteAsync(CancellationToken cancelToken)
		{
			try
			{
				await _eventStorage.Connect();

				Initialize();

				await ProcessLoop(cancelToken);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private async Task ProcessLoop(CancellationToken cancelToken)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				foreach (var processor in _eventProcessors.ToArray())
				{
					var readModelEvents = await processor.Process();
					await SendReadModelEvents(readModelEvents);
				}

				await Task.Delay(TimeSpan.FromMilliseconds(10), cancelToken);
			}
		}

		private async Task SendReadModelEvents(IEnumerable<TReadModelEvent> events)
		{
			foreach (var @event in events)
			{
				Console.WriteLine($"ReadHub: Sending Event {@event}");
				await SendReadModelEvent(@event);
			}
		}

		protected void AddProcessor<TDomainEvent>(string persistenceId, HandleEvent<TDomainEvent, TReadModelEvent> handler)
			=> _eventProcessors.Add(new EventProcessor<TDomainEvent, TReadModelEvent>(new EventReader<TDomainEvent>(_eventStorage, persistenceId), handler));
	}
}