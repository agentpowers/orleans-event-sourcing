using Orleans.Hosting;
using EventSourcingGrains.Stream;
using EventSourcingGrains.Keeplive;
using System;
using Account.Grains;
using Account.Grains.Reconciler;
using Account.Grains.ReadModelWriter;

namespace Account.Extensions
{
    public static class SiloBuilder
    {
        public static void ConfigureGrains(this ISiloBuilder builder)
        {
            // configure stream
            builder.ConfigureAggregateStream(AccountGrain.AggregateName, (aggregateStreamSettings) => 
            {
                // test
                aggregateStreamSettings.EventReceiverGrainResolverMap.Add(nameof(AccountAggregateReceiver), (aggregateEvent, grainFactory) =>
                {
                    return (IAccountAggregateReceiver)grainFactory.GetGrain(typeof(IAccountAggregateReceiver), aggregateEvent.AggregateType);
                });

                // account writer
                aggregateStreamSettings.EventReceiverGrainResolverMap.Add(nameof(AccountModelWriterGrain), (aggregateEvent, grainFactory) =>
                {
                    return (IAccountModelWriterAggregateStreamReceiver)grainFactory.GetGrain(typeof(IAccountModelWriterAggregateStreamReceiver), $"{AccountModelWriterGrain.GrainPrefix}{aggregateEvent.AggregateType}");
                });

                // account reconciler
                aggregateStreamSettings.EventReceiverGrainResolverMap.Add(nameof(AccountReconciler), (aggregateEvent, grainFactory) =>
                {
                    return (IAccountReconcilerGrain)grainFactory.GetGrain(typeof(IAccountReconcilerGrain), nameof(AccountReconcilerGrain));
                });
            });

            // configure keep alive service
            builder.ConfigureKeepAliveService((keepAliveSettings) => 
            {
                keepAliveSettings.GrainKeepAliveSettings.Add(new KeepAliveGrainSetting
                { 
                    Interval = TimeSpan.FromMinutes(60),
                    GrainResolver = (grainFactory) => 
                    {
                        var grain = grainFactory.GetGrain<IAccountReconcilerGrain>(nameof(AccountReconcilerGrain));
                        return grain;
                    }
                });
            });
        }
    }
}