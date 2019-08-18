using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaOnCore.ReadHub
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

			services.AddSignalR();

			services.AddCors(
				options => options.AddPolicy(
					"CorsPolicy", 
					builder => builder
						.AllowAnyMethod()
						.AllowAnyHeader()
						.WithOrigins("http://localhost:44300")
						.AllowCredentials()
				)
			);

			services
				.AddSingleton<IEventStorage, EventStoreStorage>()
				.AddHostedService<MeetingsListUpdateService>()
				.AddHostedService<MeetingViewUpdateService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseCors("CorsPolicy");
			app.UseMvc();

			app.UseSignalR(routes =>
			{
				routes.MapHub<MeetingsHub>("/meetingsHub");
				routes.MapHub<MeetingHub>("/meetingHub");
			});
		}
	}
}
