using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaOnCore.Actors;
using AkkaOnCore.Messages;
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

		[HttpPost]
		public IActionResult Start([FromBody]StartMeetingRequest request)
		{
			_meetingsActorRef.Tell(new StartMeetingCommand(request.Name));

			return Ok();
		}
	}
}