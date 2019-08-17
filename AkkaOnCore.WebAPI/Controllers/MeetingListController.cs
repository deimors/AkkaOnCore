using AkkaOnCore.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AkkaOnCore.WebAPI.Controllers
{
	public class MeetingListController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
		}
	}
}
