using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans;
using Orleans.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Clustering.Kubernetes;
using Grains;
using Orleans.Statistics;
using System;
using System.Net;

namespace API
{
    public class Program
    {
        const int siloPort = 11111;
        const int gatewayPort = 30000;
        public static void Main(string[] args)
        {
            var podIPAddress = Environment.GetEnvironmentVariable("MY_POD_IP");
            Console.WriteLine($"MY_POD_IP: {podIPAddress}");
            var host = new HostBuilder()
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                })
                //https://stackoverflow.com/questions/54841844/orleans-direct-client-in-asp-net-core-project/54842916#54842916
                .UseOrleans(builder =>
                {
                    // EnableDirectClient is no longer needed as it is enabled by default
                    builder.Configure<ClusterOptions>(options => 
                    {
                        options.ClusterId = "testcluster";
                        options.ServiceId = "SampleApp";
                    })
                    //.UseLocalhostClustering()
                    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Parse(podIPAddress))
                    //.ConfigureEndpoints(new Random(1).Next(10001, 10100), new Random(1).Next(20001, 20100))
                    .ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)
                    .UseKubeMembership(opt =>
                    {
                        //opt.APIEndpoint = "http://localhost:8001";
                        //opt.CertificateData = "test";
                        //opt.APIToken = "test";
                        //opt.CanCreateResources = true;
                        opt.DropResourcesOnInit = true;
                    })
                    .AddMemoryGrainStorageAsDefault()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ValueGrain).Assembly).WithReferences())
                    .ConfigureLogging(logging => logging.AddConsole())
                    .UseLinuxEnvironmentStatistics()
                    .UseDashboard(x =>
                    {
                        x.HostSelf = false;
                        x.BasePath = "/dashboard";
                        x.ScriptPath = "/api/dashboard";
                        x.CounterUpdateIntervalMs = 10000;
                    });
                })
                .Build();
            host.Run();
        }
    }
}
