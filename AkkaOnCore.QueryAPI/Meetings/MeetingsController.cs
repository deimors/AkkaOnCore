using Microsoft.AspNetCore.Mvc;

namespace AkkaOnCore.QueryAPI.Meetings
{
	[Route("api/[controller]")]
    [ApiController]
    public class MeetingsController : ControllerBase
    {
		private readonly MeetingsListReadModel _meetingsList;

		public MeetingsController(MeetingsListReadModel meetingsList)
		{
			_meetingsList = meetingsList;
		}

        // GET: api/Meetings
		[HttpGet]
		public IActionResult Get()
			=> Ok(_meetingsList.Meetings);
	}
}
