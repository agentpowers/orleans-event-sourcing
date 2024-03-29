﻿using System;
using System.Net;
using Account.Extensions;
using EventSourcingGrains.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Clustering.Kubernetes;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Account
{
    public class Program
    {
        private const int defaultSiloPort = 11111;
        private const int defaultGatewayPort = 30000;

        private static readonly bool isLocal = string.Equals(Environment.GetEnvironmentVariable("ORLEANS_ENV"), "LOCAL");
        private static readonly string siloPortEnv = Environment.GetEnvironmentVariable("SILO_PORT");
        private static readonly string gatewayPortEnv = Environment.GetEnvironmentVariable("GATEWAY_PORT");
        private static readonly string podIPAddressEnv = Environment.GetEnvironmentVariable("POD_IP");
        private static readonly string customPortEnv = Environment.GetEnvironmentVariable("CUSTOM_PORT");
        private static readonly string postgresServiceHostEnv = Environment.GetEnvironmentVariable("POSTGRES_SERVICE_HOST");
        public static string ConnectionString = $"host={(string.IsNullOrEmpty(postgresServiceHostEnv) ? "localhost" : postgresServiceHostEnv)};database=postgresdb;username=postgresadmin;password=postgrespwd;Enlist=false;Maximum Pool Size=90;";

        //https://stackoverflow.com/questions/54841844/orleans-direct-client-in-asp-net-core-project/54842916#54842916
        private static void ConfigureOrleans(ISiloBuilder builder)
        {
            builder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "account-cluster";
                options.ServiceId = "ACCOUNT";
            })
            .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Parse(podIPAddressEnv))
            .ConfigureEndpoints(siloPort: defaultSiloPort, gatewayPort: defaultGatewayPort)
            .UseKubeMembership()
            .AddMemoryGrainStorageAsDefault()
            .AddGrainService<KeepAliveService>()
            .UseDashboard(x =>
            {
                x.HostSelf = false;
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
                options.ClusterId = "account-cluster";
                options.ServiceId = "ACCOUNT";
            })
            .ConfigureEndpoints(
                string.IsNullOrEmpty(siloPortEnv) ? defaultSiloPort : int.Parse(siloPortEnv),
                string.IsNullOrEmpty(gatewayPortEnv) ? defaultGatewayPort : int.Parse(gatewayPortEnv))
            .UseAdoNetClustering(options =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = ConnectionString;
            })
            .AddMemoryGrainStorageAsDefault()
            // .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(AccountGrain).Assembly).WithReferences())
            .AddGrainService<KeepAliveService>()
            .UseDashboard(x =>
            {
                x.HostSelf = false;
                x.BasePath = "/dashboard";
                x.CounterUpdateIntervalMs = 10000;
            });
            // .AddPrometheusTelemetryConsumer();

            builder.ConfigureGrains();
        }

        public static void Main(string[] args)
        {
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
