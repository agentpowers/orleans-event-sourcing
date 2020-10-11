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
        const int defaultSiloPort = 11111;
        const int defaultGatewayPort = 30000;

        private static readonly bool isLocal = string.Equals(Environment.GetEnvironmentVariable("ORLEANS_ENV"), "LOCAL");
        private static readonly string siloPortEnv = Environment.GetEnvironmentVariable("SILO_PORT");
        private static readonly string gatewayPortEnv = Environment.GetEnvironmentVariable("GATEWAY_PORT");
        private static readonly string podIPAddressEnv = Environment.GetEnvironmentVariable("POD_IP");
        private static readonly string customPortEnv = Environment.GetEnvironmentVariable("CUSTOM_PORT");
        private static readonly string postgresServiceHostEnv = Environment.GetEnvironmentVariable("POSTGRES_SERVICE_HOST");
        private static readonly string connectionString = $"host={(string.IsNullOrEmpty(postgresServiceHostEnv) ? "localhost" : postgresServiceHostEnv)};database=postgresdb;username=postgresadmin;password=postgrespwd;Enlist=false;";
        private static void ConfigureOrleans(ISiloBuilder builder)
        {
            builder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "caching-cluster";
                options.ServiceId = "CACHING";
            })
            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Parse(podIPAddressEnv))
            .ConfigureEndpoints(siloPort: defaultSiloPort, gatewayPort: defaultGatewayPort)
            .UseAdoNetClustering(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddMemoryGrainStorageAsDefault()
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(CacheGrain<>).Assembly).WithReferences())
            .UseLinuxEnvironmentStatistics()
.UseDashboard(x =>
{
    x.HostSelf = false;
    x.BasePath = "/dashboard";
    x.ScriptPath = "/api/dashboard";
    x.CounterUpdateIntervalMs = 10000;
});
        }

        private static void ConfigureLocalOrleans(ISiloBuilder builder)
        {
            builder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "caching-cluster";
                options.ServiceId = "CACHING";
            })
            .ConfigureEndpoints(
                string.IsNullOrEmpty(siloPortEnv) ? defaultSiloPort : int.Parse(siloPortEnv),
                string.IsNullOrEmpty(gatewayPortEnv) ? defaultGatewayPort : int.Parse(gatewayPortEnv))
            .UseAdoNetClustering(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .AddMemoryGrainStorageAsDefault()
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(CacheGrain<>).Assembly).WithReferences())
.UseDashboard(x =>
{
    x.HostSelf = false;
    x.BasePath = "/dashboard";
    x.CounterUpdateIntervalMs = 10000;
});
        }

        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting {(isLocal ? "LOCAL" : "K8S")} config");
            var hostBuilder =
                Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
.ConfigureWebHostDefaults(builder =>
{
    builder.UseStartup<Startup>();
    //use custom port if provided for kestrel
    if (!string.IsNullOrEmpty(customPortEnv))
    {
        Console.WriteLine($"Starting Kestrel in port {customPortEnv}");
        builder.UseKestrel(kestrelOptions =>
        {
            kestrelOptions.ListenAnyIP(int.Parse(customPortEnv));
        });
    }
});
            // configure
            if (isLocal)
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
