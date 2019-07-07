using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using AkkaOnCore.APICommon;
using AkkaOnCore.QueryAPI.Meetings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AkkaOnCore.QueryAPI
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

			services.AddCors(
				options => options.AddPolicy(
					"CorsPolicy",
					builder => builder
						.AllowAnyMethod()
						.AllowAnyHeader()
						.AllowCredentials()
						.WithOrigins("https://localhost:44331")
				)
			);

			services.AddSingleton(_ => ActorSystem.Create("meetingsquery", LoadAkkaConfig("akka.conf")));

			services.AddSingleton<MeetingsListReadModel>();
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
			app.UseCors("CorsPolicy");

			lifetime.RegisterActorSystem(app);
		}
	}
}
