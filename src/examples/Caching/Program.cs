using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Statistics;
using System;
using System.Net;
using Caching.Grains;

namespace Caching
{
    public class Program
    {
        const int siloPort = 11111;
        const int gatewayPort = 30000;

        public static bool isLocal = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development");

        public static string connectionString = Program.isLocal 
				? "host=localhost;database=EventSourcing;username=orleans;password=orleans"
				: $"host={Environment.GetEnvironmentVariable("POSTGRES_SERVICE_HOST")};database=postgresdb;username=postgresadmin;password=postgrespwd";
        private static void ConfigureOrleans(ISiloBuilder builder)
        {
            // get injected pod ip address 
            var podIPAddress = Environment.GetEnvironmentVariable("POD_IP");
            builder.Configure<ClusterOptions>(options => 
            {
                options.ClusterId = "account-cluster";
                options.ServiceId = "ACCOUNT";
            })
            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Parse(podIPAddress))
            .ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)
            .UseAdoNetClustering(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddMemoryGrainStorageAsDefault()
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(CacheGrain<>).Assembly).WithReferences())
            .UseLinuxEnvironmentStatistics()
            .UseDashboard(x =>
             {
                x.HostSelf = false;
                x.BasePath = "/dashboard";
                x.ScriptPath = "/api/dashboard";
                x.CounterUpdateIntervalMs = 10000;
            });
        }
        
        private static void ConfigureLocalOrleans(ISiloBuilder builder)
        {
            builder.Configure<ClusterOptions>(options => 
            {
                options.ClusterId = "account-cluster";
                options.ServiceId = "ACCOUNT";
            })
            .ConfigureEndpoints(new Random(1).Next(10001, 10100), new Random(1).Next(20001, 20100))
            .ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)
            .UseAdoNetClustering(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddMemoryGrainStorageAsDefault()
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(CacheGrain<>).Assembly).WithReferences())
            .UseDashboard(x =>
             {
                x.HostSelf = false;
                x.BasePath = "/dashboard";
                x.CounterUpdateIntervalMs = 10000;
             });
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
            // configure
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
