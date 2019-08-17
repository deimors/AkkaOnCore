using System;
using Microsoft.AspNetCore.Mvc;

namespace AkkaOnCore.WebAPI.Controllers
{
	public class MeetingController : Controller
	{
		public IActionResult Index(Guid id)
		{
			ViewBag.MeetingId = id;

			return View();
		}
	}
}