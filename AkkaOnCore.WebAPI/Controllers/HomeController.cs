using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AkkaOnCore.Models;
using AkkaOnCore.QueryAPI.Messages;
using Newtonsoft.Json;

namespace AkkaOnCore.Controllers
{
	public class HomeController : Controller
	{
		private readonly IHttpClientFactory _clientFactory;

		public HomeController(IHttpClientFactory clientFactory)
		{
			_clientFactory = clientFactory;
		}

		public async Task<IActionResult> Index()
		{
			var client = _clientFactory.CreateClient("query");

			var result = await client.GetAsync("/api/meetings");

			ViewBag.Meetings = result.IsSuccessStatusCode 
				? JsonConvert.DeserializeObject<IEnumerable<MeetingListEntry>>(await result.Content.ReadAsStringAsync()) 
				: Enumerable.Empty<MeetingListEntry>();

			return View();
		}

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
