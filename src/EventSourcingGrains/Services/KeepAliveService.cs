using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Services;
using EventSourcingGrains.Stream;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using EventSourcingGrains.Keeplive;

namespace EventSourcingGrains.Services
{
    public interface IKeepAliveService : IGrainService { }
    public class KeepAliveService : GrainService, IKeepAliveService
    {
        private IServiceProvider _serviceProvider;
        private readonly IGrainFactory _grainFactory;
        private static readonly TimeSpan InitialInterval = TimeSpan.Zero;
        private static readonly TimeSpan AggregateStreamInterval = TimeSpan.FromMinutes(10);

        public KeepAliveService(GrainId grainId, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) : base(grainId, silo, loggerFactory)
        {
            _grainFactory = grainFactory;
        }

        public override async Task Init(IServiceProvider serviceProvider)
        {
            await base.Init(serviceProvider);
            _serviceProvider = serviceProvider;
        }

        public override async Task Start()
        {
            await base.Start();
            var keepAliveSettings = _serviceProvider.GetService<IKeepAliveSettings>();

            if (keepAliveSettings != null)
            {
                foreach (var setting in keepAliveSettings.GrainKeepAliveSettings)
                {
                    RegisterTimer(HandlePing, setting, InitialInterval, setting.Interval);
                }
            }

            //  add AggregateStreams (INTERNAL)
            var aggregateNames = _serviceProvider.GetServices<IAggregateStreamSettings>().Select(g => g.AggregateName).ToArray();
            foreach (var aggregateName in aggregateNames)
            {
                RegisterTimer(HandleAggregateStreamPing, aggregateName, InitialInterval, AggregateStreamInterval);
            }
        }

        private async Task HandlePing(object arg)
        {
            var keepAliveGrainSetting = (KeepAliveGrainSetting)arg;
            var aggregateStreamGrain = keepAliveGrainSetting.GrainResolver(_grainFactory);
            await aggregateStreamGrain.Ping();
        }

        private async Task HandleAggregateStreamPing(object arg)
        {
            var aggregateStreamGrain = _grainFactory.GetGrain<IAggregateStreamGrain>((string)arg);
            await aggregateStreamGrain.Ping();
        }
    }
}