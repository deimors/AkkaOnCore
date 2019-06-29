using Akka.Actor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaOnCore.APICommon
{
	public static class ApplicationLifetimeExtensions
	{
		public static void RegisterActorSystem(this IApplicationLifetime lifetime, IApplicationBuilder app)
		{
			lifetime.ApplicationStarted.Register(() => app.ApplicationServices.GetService<ActorSystem>());
			lifetime.ApplicationStopping.Register(() => app.ApplicationServices.GetService<ActorSystem>().Terminate().Wait());
		}
	}
}