using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaOnCore.Actors;
using Microsoft.AspNetCore.Mvc;

namespace AkkaOnCore.API.Meetings
{
	[Route("api/[controller]")]
	[ApiController]
	public class MeetingsController : Controller
	{
		private readonly IActorRef _meetingsActorRef;

		public MeetingsController(MeetingsActorRefFactory meetingsRefFactory)
		{
			_meetingsActorRef = meetingsRefFactory();
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var result = await _meetingsActorRef.Ask<List<KeyValuePair<Guid, string>>>(new GetMeetingsQuery());

			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Start([FromBody]StartMeetingRequest request)
		{
			var result = await _meetingsActorRef.Ask<Guid>(new StartMeetingCommand(request.Name));

			return Ok(result);
		}
	}
}