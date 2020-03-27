using Orleans.Hosting;
using EventSourcing.Stream;
using Grains.Account;
using Grains.Account.ReadModelWriter;

namespace Grains.Extensions
{
    public static class AggregateStream
    {
        public static void ConfigureAggregateStream(this ISiloBuilder builder)
        {
            builder.ConfigureAggregateStream(AccountGrain.AggregateName, (aggregateStreamSettings) => 
            {
                aggregateStreamSettings.EventReceiverGrainResolverMap.Add(nameof(AccountAggregateReceiver), (aggregateEvent, grainFactory) =>
                {
                    return (IAccountAggregateReceiver)grainFactory.GetGrain(typeof(IAccountAggregateReceiver), aggregateEvent.AggregateType);
                });

                aggregateStreamSettings.EventReceiverGrainResolverMap.Add(nameof(AccountModelWriterGrain), (aggregateEvent, grainFactory) =>
                {
                    return (IAccountModelWriterAggregateStreamReceiver)grainFactory.GetGrain(typeof(IAccountModelWriterAggregateStreamReceiver), $"{AccountModelWriterGrain.GrainPrefix}{aggregateEvent.AggregateType}");
                });
            });
        }
    }
}