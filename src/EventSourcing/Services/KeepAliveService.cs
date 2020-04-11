using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Orleans.Core;
using Microsoft.Extensions.Logging;
using Orleans.Services;
using EventSourcing.Stream;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using EventSourcing.Keeplive;

namespace EventSourcing.Services
{
    public interface IKeepAliveService: IGrainService{}
    public class KeepAliveService : GrainService, IKeepAliveService
    {
        private readonly IGrainFactory _grainFactory;
        
        public KeepAliveService(IGrainIdentity grainId, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) : base(grainId, silo, loggerFactory)
        {
            _grainFactory = grainFactory;
        }

        public override async Task Init(IServiceProvider serviceProvider)
        {
            var keepAliveSettings = serviceProvider.GetService<IKeepAliveSettings>();

            if (keepAliveSettings != null)
            {
                foreach (var setting in keepAliveSettings.GrainKeepAliveSettings)
                {
                    this.RegisterTimer(HandlePing, setting, TimeSpan.Zero, setting.Interval);
                }
            }

            //  add AggregateStreams (INTERNAL)
            var aggregateNames = serviceProvider.GetServices<IAggregateStreamSettings>().Select(g => g.AggregateName).ToArray();
            foreach (var aggregateName in aggregateNames)
            {
                this.RegisterTimer(HandleAggregateStreamPing, aggregateName, TimeSpan.Zero, TimeSpan.FromMinutes(60));
            }

            await base.Init(serviceProvider);
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