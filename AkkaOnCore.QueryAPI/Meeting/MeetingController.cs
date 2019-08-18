using AkkaOnCore.ReadModel.Meeting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using Functional;

namespace AkkaOnCore.QueryAPI.Meeting
{
	public class MeetingViewReadModelCollection
	{
		private readonly ConcurrentDictionary<Guid, MeetingViewReadModel> _collection = new ConcurrentDictionary<Guid, MeetingViewReadModel>();

		public void Add(Guid meetingId, string name)
		{
			_collection.TryAdd(meetingId, new MeetingViewReadModel(meetingId, name));
		}

		public Option<MeetingViewReadModel> this[Guid meetingId] 
			=> _collection.TryGetValue(meetingId, out var value) 
				? Option.Some(value) 
				: Option.None<MeetingViewReadModel>();
	}

	[Route("[controller]")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
		private readonly MeetingViewReadModelCollection _readModelCollection;

		public MeetingController(MeetingViewReadModelCollection readModelCollection)
		{
			_readModelCollection = readModelCollection ?? throw new ArgumentNullException(nameof(readModelCollection));
		}

		[HttpGet("{meetingId}")]
		public IActionResult GetMeeting(string meetingId)
			=> _readModelCollection[Guid.Parse(meetingId)].Match<IActionResult>(Ok, NotFound);
	}
}