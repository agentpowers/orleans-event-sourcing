using System;
using EventSourcingGrains.Grains;
using EventSourcingGrains.Keeplive;
using EventSourcingGrains.Stream;
using Orleans.Hosting;
using Saga.Grains.EventSourcing;
using Saga.Grains.Manager;

namespace SagaExample.Extensions
{
    public static class SiloBuilder
    {
        public static void ConfigureGrains(this ISiloBuilder builder)
        {
            // configure stream
            builder.ConfigureAggregateStream(SagaGrain<object>.AggregateName, (aggregateStreamSettings) =>
            {
                // saga manager
                aggregateStreamSettings.EventDispatcherSettingsMap.Add(nameof(SagaManagerGrain), new EventDispatcherSettings
                {
                    ReceiverGrainResolver = (aggregateEvent, grainFactory) =>
                    {
                        return (ISagaManagerGrain)grainFactory.GetGrain(typeof(ISagaManagerGrain), nameof(SagaManagerGrain));
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
                        var grain = grainFactory.GetGrain<ISagaManagerGrain>(nameof(SagaManagerGrain));
                        return grain;
                    }
                });
            });

            // eventsourcing grain configuration
            builder.ConfigureEventSourcingGrains((settings) =>
            {
                // saga grain
                settings.Add(SagaGrain<object>.AggregateName, new EventSourceGrainSetting());
                // account reconciler grain
                settings.Add(SagaManagerGrain.AggregateName, new EventSourceGrainSetting());
            });
        }
    }
}
