using System;
using Akka.Actor;
using Akka.Configuration;
using AkkaOnCore.Actors;
using AkkaOnCore.APICommon;
using AkkaOnCore.CommandAPI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace AkkaOnCore.CommandAPI
{
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddSingleton(_ => ActorSystem.Create("meetings", LoadAkkaConfig("akka.conf")));

			services.AddSingleton<MeetingsActorRefFactory>(serviceProvider =>
			{
				var actorRef = serviceProvider.GetService<ActorSystem>().ActorOf(MeetingsActor.CreateProps(), "Meetings");

				return () => actorRef;
			});

			services.AddSingleton<MeetingActorRefFactory>(
				serviceProvider => 
					meetingId => serviceProvider
						.GetService<ActorSystem>()
						.ActorSelection($"/user/Meetings/{meetingId}")
						.ResolveOne(TimeSpan.FromSeconds(5))
			);
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
				app.UseHsts();
			}

			app.UseMvc();

			lifetime.RegisterActorSystem(app);
		}
	}
}
