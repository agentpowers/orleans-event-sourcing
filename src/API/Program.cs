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
using EventSourcing.Stream;

namespace API
{
    public class Program
    {
        const int siloPort = 11111;
        const int gatewayPort = 30000;

        public static bool isLocal = string.Equals(Environment.GetEnvironmentVariable("ORLEANS_ENV"), "LOCAL");

        private static void ConfigureAggregateStream(ISiloBuilder builder)
        {
            builder.ConfigureAggregateStream(AccountGrain.AggregateName, (aggregateStreamSettings) => 
            {
                aggregateStreamSettings.EventReceiverGrainResolverMap.Add("test", (aggregateEvent, grainFactory) =>
                {
                    return (IAggregateStreamReceiver)grainFactory.GetGrain(typeof(IAggregateStreamReceiver), aggregateEvent.AggregateType);
                });

                aggregateStreamSettings.EventReceiverGrainResolverMap.Add("test2", (aggregateEvent, grainFactory) =>
                {
                    return (IAggregateStreamReceiver)grainFactory.GetGrain(typeof(IAggregateStreamReceiver), "test2:" + aggregateEvent.AggregateType);
                });
            });
        }
        
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
            .AddGrainService<EventSourcing.Services.AggregateStreamKeepAliveService>()
            .ConfigureLogging(logging => logging.AddConsole())
            .UseLinuxEnvironmentStatistics()
            .UseDashboard(x =>
             {
                x.HostSelf = false;
                x.BasePath = "/dashboard";
                x.ScriptPath = "/api/dashboard";
                x.CounterUpdateIntervalMs = 10000;
            });

            ConfigureAggregateStream(builder);
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
            .AddGrainService<EventSourcing.Services.AggregateStreamKeepAliveService>()
            .ConfigureLogging(logging => logging.AddConsole())
            .UseDashboard(x =>
             {
                x.HostSelf = false;
                x.BasePath = "/dashboard";
                x.CounterUpdateIntervalMs = 10000;
             });

            ConfigureAggregateStream(builder);
        }
        
        public static void Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
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
