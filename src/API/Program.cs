using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Clustering.Kubernetes;
using Orleans.Statistics;
using System;
using System.Net;
using Grains.Account;
using Grains.Extensions;

namespace API
{
    public class Program
    {
        const int siloPort = 11111;
        const int gatewayPort = 30000;

        public static bool isLocal = string.Equals(Environment.GetEnvironmentVariable("ORLEANS_ENV"), "LOCAL");

        
        //https://stackoverflow.com/questions/54841844/orleans-direct-client-in-asp-net-core-project/54842916#54842916
        private static void ConfigureOrleans(ISiloBuilder builder)
        {
            // get injected pod ip address 
            var podIPAddress = Environment.GetEnvironmentVariable("POD_IP");
            builder.Configure<ClusterOptions>(options => 
            {
                options.ClusterId = "api-cluster";
                options.ServiceId = "API";
            })
            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Parse(podIPAddress))
            .ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)
            .UseKubeMembership(opt =>
            {
                opt.DropResourcesOnInit = true;
            })
            .AddMemoryGrainStorageAsDefault()
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AccountGrain).Assembly).WithReferences())
            .AddGrainService<EventSourcing.Services.KeepAliveService>()
            .UseLinuxEnvironmentStatistics()
            .UseDashboard(x =>
             {
                x.HostSelf = false;
                x.BasePath = "/dashboard";
                x.ScriptPath = "/api/dashboard";
                x.CounterUpdateIntervalMs = 10000;
            });

            builder.ConfigureGrains();
        }
        
        private static void ConfigureLocalOrleans(ISiloBuilder builder)
        {
            builder.Configure<ClusterOptions>(options => 
            {
                options.ClusterId = "api-cluster";
                options.ServiceId = "API";
            })
            .UseLocalhostClustering()
            .ConfigureEndpoints(new Random(1).Next(10001, 10100), new Random(1).Next(20001, 20100))
            .ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)
            .AddMemoryGrainStorageAsDefault()
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AccountGrain).Assembly).WithReferences())
            .AddGrainService<EventSourcing.Services.KeepAliveService>()
            .UseDashboard(x =>
             {
                x.HostSelf = false;
                x.BasePath = "/dashboard";
                x.CounterUpdateIntervalMs = 10000;
             });

            builder.ConfigureGrains();
        }
        
        public static void Main(string[] args)
        {
            var hostBuilder = 
                Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                });
            if(isLocal)
            {
                hostBuilder.UseOrleans(ConfigureLocalOrleans);
            }
            else
            {
                hostBuilder.UseOrleans(ConfigureOrleans);
            }

            hostBuilder.Build().Run();
        }
    }
}
