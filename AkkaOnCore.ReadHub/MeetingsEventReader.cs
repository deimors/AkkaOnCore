using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkkaOnCore.Messages;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace AkkaOnCore.ReadHub
{
	public class MeetingsEventReader : BackgroundService
	{
		private readonly IHubContext<MeetingsHub, IMeetings> _meetingsHub;
		private long _nextStartIndex = 0;

		public MeetingsEventReader(IHubContext<MeetingsHub, IMeetings> meetingsHub)
		{
			_meetingsHub = meetingsHub;
		}

		protected override async Task ExecuteAsync(CancellationToken cancelToken)
		{
			try
			{
				var connection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@eventstore-node:1113");

				await connection.ConnectAsync();

				while (!cancelToken.IsCancellationRequested)
				{
					var slice = await connection.ReadStreamEventsForwardAsync("MeetingsActor", _nextStartIndex, 128, false);

					_nextStartIndex = slice.LastEventNumber + 1;

					foreach (var resolvedEvent in slice.Events)
					{
						var meetingsEvent = DeserializeEvent(resolvedEvent);

						await meetingsEvent.Match(
							meetingStarted => _meetingsHub.Clients.All.OnMeetingAddedToList(meetingStarted.MeetingId.ToString(), meetingStarted.Name)
						);
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