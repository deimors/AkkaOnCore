using System;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaOnCore.Messages;
using Functional;
using Microsoft.AspNetCore.Mvc;

namespace AkkaOnCore.CommandAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class MeetingController : Controller
	{
		private readonly MeetingActorRefFactory _meetingRefFactory;

		public MeetingController(MeetingActorRefFactory meetingRefFactory)
		{
			_meetingRefFactory = meetingRefFactory ?? throw new ArgumentNullException(nameof(meetingRefFactory));
		}

		[HttpPost("{meetingId}/addtoagenda")]
		public async Task<IActionResult> AddToAgenda(Guid meetingId, [FromBody]AddToAgendaRequest request)
		{
			var actorRef = await _meetingRefFactory(meetingId);

			var result = await actorRef.Ask<Result<Unit, MeetingCommandError>>(new MeetingCommand.AddToAgenda(request.Description));

			return result.Match(_ => Ok() as IActionResult, error => BadRequest(error.Message));
		}
	}
}