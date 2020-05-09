﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Microsoft.Extensions.Hosting;
using System;
using Account.Extensions;

namespace Account
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			var dbHost = Environment.GetEnvironmentVariable("POSTGRES_SERVICE_HOST");
			// configure postgres connection string
			var postgresConnectionString = Program.isLocal 
				? "host=localhost;database=EventSourcing;username=orleans;password=orleans"
				: $"host={dbHost};database=postgresdb;username=postgresadmin;password=postgrespwd";
			// add grain service
			services.AddGrainServices(postgresConnectionString);
			// add controllers
			services.AddControllers();
			// add services for dashboard
			services.AddServicesForSelfHostedDashboard();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseRouting();

			app.UseOrleansDashboard(new OrleansDashboard.DashboardOptions 
			{ 
				BasePath = "/dashboard"
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
			});
		}
	}
}