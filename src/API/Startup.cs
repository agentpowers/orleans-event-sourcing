using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Microsoft.Extensions.Hosting;
using EventSourcing.Extensions;
using System;

namespace API
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
            // var postgresConnectionString = "host=localhost;database=EventSourcing;username=orleans;password=orleans";
            var postgresConnectionString = $"host={dbHost};database=postgresdb;username=postgresadmin;password=postgrespwd";
            // add evensourcing related services
            services.AddEventSourcing(postgresConnectionString);
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
