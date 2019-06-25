using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using AkkaOnCore.Actors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaOnCore
{
	public delegate IActorRef MeetingsActorRefFactory();

	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});


			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddSingleton(_ => ActorSystem.Create("meetings", LoadAkkaConfig("akka.conf")));

			services.AddSingleton<MeetingsActorRefFactory>(serviceProvider =>
			{
				var actorRef = serviceProvider.GetService<ActorSystem>().ActorOf(MeetingsActor.CreateProps());

				return () => actorRef;
			});
		}

		public static Config LoadAkkaConfig(string filename)
			=> File.Exists(filename) 
				? ConfigurationFactory.ParseString(File.ReadAllText(filename)) 
				: Config.Empty;

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

			lifetime.ApplicationStarted.Register(() => app.ApplicationServices.GetService<ActorSystem>());
			lifetime.ApplicationStopping.Register(() => app.ApplicationServices.GetService<ActorSystem>().Terminate().Wait());
		}
	}
}
