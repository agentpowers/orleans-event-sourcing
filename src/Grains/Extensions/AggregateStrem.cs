using Orleans.Hosting;
using EventSourcing.Stream;
using Grains.Account;

namespace Grains.Extensions
{
    public static class AggregateStream
    {
        public static void ConfigureAggregateStream(this ISiloBuilder builder)
        {
            builder.ConfigureAggregateStream(AccountGrain.AggregateName, (aggregateStreamSettings) => 
            {
                //var accountAggregateReceiverPrefix = typeof(AccountAggregateReceiver).FullName;
                aggregateStreamSettings.EventReceiverGrainResolverMap.Add(nameof(AccountAggregateReceiver), (aggregateEvent, grainFactory) =>
                {
                    return (IAccountAggregateReceiver)grainFactory.GetGrain(typeof(IAccountAggregateReceiver), aggregateEvent.AggregateType);
                });

                aggregateStreamSettings.EventReceiverGrainResolverMap.Add(nameof(AccountModelWriter), (aggregateEvent, grainFactory) =>
                {
                    return (IAccountModelAggregateStreamReceiver)grainFactory.GetGrain(typeof(IAccountModelAggregateStreamReceiver), aggregateEvent.AggregateType);
                });
            });
        }
    }
}