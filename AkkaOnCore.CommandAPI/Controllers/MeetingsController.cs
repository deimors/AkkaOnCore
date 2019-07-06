using System.Threading.Tasks;
using Akka.Actor;
using AkkaOnCore.Messages;
using Functional;
using Microsoft.AspNetCore.Mvc;

namespace AkkaOnCore.CommandAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class MeetingsController : Controller
	{
		private readonly IActorRef _meetingsActorRef;

		public MeetingsController(MeetingsActorRefFactory meetingsRefFactory)
		{
			_meetingsActorRef = meetingsRefFactory();
		}

		[HttpPost]
		public async Task<IActionResult> Start([FromBody]StartMeetingRequest request)
			=> (await _meetingsActorRef.Ask<Result<Unit, MeetingsCommandError>>(new MeetingsCommand.StartMeeting(request.Name)))
				.Match(_ => Ok() as IActionResult, error => BadRequest(error.Message));
	}
}