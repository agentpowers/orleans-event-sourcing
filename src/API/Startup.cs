using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GrainInterfaces;
using Orleans;
using Orleans.Clustering.Kubernetes;
using Orleans.Configuration;
using Orleans.Runtime;
using Microsoft.Extensions.Hosting;
using Persistance;

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
            services.AddTransient<IRepository, Repository>();
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
    // public class Startup
    // {
    //     public Startup(IConfiguration configuration)
    //     {
    //         Configuration = configuration;
    //     }

    //     public IConfiguration Configuration { get; }

    //     // This method gets called by the runtime. Use this method to add services to the container.
    //     public void ConfigureServices(IServiceCollection services)
    //     {
    //         services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    //         services.AddSingleton<IClusterClient>(a => StartClientWithRetries().Result);
    //     }

    //     // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    //     public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    //     {
    //         if (env.IsDevelopment())
    //         {
    //             app.UseDeveloperExceptionPage();
    //         }
    //         else
    //         {
    //             //app.UseHsts();
    //         }

    //         //app.UseHttpsRedirection();
    //         app.UseMvc();
    //     }

    //     private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
    //     {
    //         int attempt = 0;
    //         IClusterClient client;
    //         while (true)
    //         {
    //             try
    //             {
    //                 client = new ClientBuilder()
    //                     .Configure<ClusterOptions>(options => 
    //                     {
    //                         options.ClusterId = "testcluster";
    //                         options.ServiceId = "SampleApp";
    //                     })
    //                     //.UseLocalhostClustering()
    //                     .UseKubeGatewayListProvider()
    //                     .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IValueGrain).Assembly).WithReferences())
    //                     .ConfigureLogging(logging => logging.AddConsole())
    //                     .Build();

    //                 await client.Connect();
    //                 Console.WriteLine("Client successfully connect to silo host");
    //                 break;
    //             }
    //             catch (SiloUnavailableException)
    //             {
    //                 attempt++;
    //                 Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
    //                 if (attempt > initializeAttemptsBeforeFailing)
    //                 {
    //                     throw;
    //                 }
    //                 await Task.Delay(TimeSpan.FromSeconds(4));
    //             }
    //         }

    //         return client;
    //     }
    // }
}
