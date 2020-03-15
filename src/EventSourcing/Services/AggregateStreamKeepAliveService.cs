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

namespace EventSourcing.Services
{
    public interface IAggregateStreamKeepAliveService: IGrainService{}
    public class AggregateStreamKeepAliveService : GrainService, IAggregateStreamKeepAliveService
    {
        private readonly IGrainFactory _grainFactory;

        private string[] _aggregateNames;
        
        public AggregateStreamKeepAliveService(IGrainIdentity grainId, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) : base(grainId, silo, loggerFactory)
        {
            _grainFactory = grainFactory;
        }

        public override async Task Init(IServiceProvider serviceProvider)
        {
            this.RegisterTimer(AggregateStreamGrainPingHandler, null, TimeSpan.Zero, TimeSpan.FromMinutes(60));

            _aggregateNames = serviceProvider.GetServices<IAggregateStreamSettings>().Select(g => g.AggregateName).ToArray();

            await base.Init(serviceProvider);
        }

        public async Task AggregateStreamGrainPingHandler(object args)
        {
            foreach (var aggregateName in _aggregateNames)
            {
                var aggregateStreamGrain = _grainFactory.GetGrain<IAggregateStream>(aggregateName);
                await aggregateStreamGrain.Ping();
            }
        }
    }
}