using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;
using Orleans.Core;
using Microsoft.Extensions.Logging;
using Orleans.Services;
using EventSourcing.Stream;

namespace EventSourcing.Services
{
    public interface IAggregateStreamKeepAliveService: IGrainService{}
    public class AggregateStreamKeepAliveService : GrainService, IAggregateStreamKeepAliveService
    {
        private readonly IGrainFactory _grainFactory;
        
        public AggregateStreamKeepAliveService(IGrainIdentity grainId, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) : base(grainId, silo, loggerFactory)
        {
            _grainFactory = grainFactory;
        }

        public override async Task Init(IServiceProvider serviceProvider)
        {
            this.RegisterTimer(AggregateStreamGrainPingHandler, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30));

            await base.Init(serviceProvider);
        }

        public async Task AggregateStreamGrainPingHandler(object args)
        {
            foreach (var streamType in AggregateStreamConfig.StreamTypes)
            {
                var aggregateStreamGrain = _grainFactory.GetGrain<IAggregateStream>(streamType);
                await aggregateStreamGrain.Ping();
            }
        }
    }
}