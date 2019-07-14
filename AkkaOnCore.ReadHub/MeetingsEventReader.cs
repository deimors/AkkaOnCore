using System;
using System.IO;
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
	public class MeetingsEventReader : BackgroundService
	{
		private readonly IHubContext<MeetingsHub, IMeetings> _meetingsHub;
		private readonly ILogger<MeetingsEventReader> _logger;

		private long _nextStartIndex;
		private bool _isConnected;

		public MeetingsEventReader(IHubContext<MeetingsHub, IMeetings> meetingsHub, ILogger<MeetingsEventReader> logger)
		{
			_meetingsHub = meetingsHub;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken cancelToken)
		{
			try
			{
				var connection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@eventstore-node:1113; MaxReconnections=-1");

				connection.Connected += OnConnected;
				connection.Disconnected += OnDisconnected;
				connection.Closed += OnClosed;
				connection.ErrorOccurred += OnErrorOccurred;

				await connection.ConnectAsync();

				_logger.LogDebug("After ConnectAsync()");

				while (!cancelToken.IsCancellationRequested)
				{
					if (_isConnected)
					{
						await ProcessEvents(connection);
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

		private async Task ProcessEvents(IEventStoreConnection connection)
		{
			var slice = await connection.ReadStreamEventsForwardAsync("MeetingsActor", _nextStartIndex, 128, false);

			_nextStartIndex = slice.LastEventNumber + 1;

			foreach (var resolvedEvent in slice.Events)
			{
				_logger.LogInformation($"Received {resolvedEvent.Event.EventType} ({resolvedEvent.Event.EventId})");

				var meetingsEvent = DeserializeEvent(resolvedEvent);

				await meetingsEvent.Match(
					meetingStarted
						=> _meetingsHub.Clients.All.OnMeetingAddedToList(meetingStarted.MeetingId.ToString(),
							meetingStarted.Name)
				);
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
		}

		private static MeetingsEvent DeserializeEvent(ResolvedEvent resolvedEvent)
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
				var deserialized = JsonSerializer.Create().Deserialize((TextReader) reader, eventType);
				return (MeetingsEvent) deserialized;
			}
		}
	}
}