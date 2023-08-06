using System;
using Account.Grains;
using Account.Grains.ReadModelWriter;
using Account.Grains.Reconciler;
using EventSourcingGrains.Grains;
using EventSourcingGrains.Keeplive;
using EventSourcingGrains.Stream;
using Orleans.Hosting;

namespace Account.Extensions
{
    public static class SiloBuilder
    {
        public static void ConfigureGrains(this ISiloBuilder builder)
        {
            // configure stream
            builder.ConfigureAggregateStream(AccountGrain.AggregateName, (aggregateStreamSettings) =>
            {
                // account writer
                aggregateStreamSettings.EventDispatcherSettingsMap.Add(nameof(AccountModelWriterGrain), new EventDispatcherSettings
                {
                    ReceiverGrainResolver = (aggregateEvent, grainFactory) =>
                    {
                        return (IAccountModelWriterAggregateStreamReceiver)grainFactory.GetGrain(typeof(IAccountModelWriterAggregateStreamReceiver), $"{AccountModelWriterGrain.GrainPrefix}{aggregateEvent.AggregateType}");
                    }
                });

                // account reconciler
                aggregateStreamSettings.EventDispatcherSettingsMap.Add(nameof(AccountReconciler), new EventDispatcherSettings
                {
                    ReceiverGrainResolver = (aggregateEvent, grainFactory) =>
                    {
                        return (IAccountReconcilerGrain)grainFactory.GetGrain(typeof(IAccountReconcilerGrain), nameof(AccountReconcilerGrain));
                    }
                });
            });

            // configure keep alive service
            builder.ConfigureKeepAliveService((keepAliveSettings) =>
            {
                keepAliveSettings.GrainKeepAliveSettings.Add(new KeepAliveGrainSetting
                {
                    Interval = TimeSpan.FromMinutes(10),
                    GrainResolver = (grainFactory) =>
                    {
                        var grain = grainFactory.GetGrain<IAccountReconcilerGrain>(nameof(AccountReconcilerGrain));
                        return grain;
                    }
                });
            });

            // eventsourcing grain configuration
            builder.ConfigureEventSourcingGrains((settings) =>
            {
                // account grain
                settings.Add(AccountGrain.AggregateName, new EventSourceGrainSetting());
                // account reconciler grain
                settings.Add(AccountReconcilerGrain.AggregateName, new EventSourceGrainSetting());
            });
        }
    }
}
