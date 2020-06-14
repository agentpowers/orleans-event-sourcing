using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace EventSourcingGrains.Grains
{
    public static class ConfigureEventSourcingGrainExtensions
    {
        public static ISiloBuilder ConfigureEventSourcingGrains(this ISiloBuilder builder, Action<IEventSourceGrainSettingsMap> configure)
        {
            var settings = new EventSourceGrainSettingsMap();

            configure.Invoke(settings);

            // add internal EventSourceGrains
            // add AggregateStreamDispatcherGrain settings
            settings.Add(AggregateStreamDispatcherGrain.AggregateName, new EventSourceGrainSetting());

            builder.ConfigureServices((hostBuilder, serviceCollection) => 
            {
                serviceCollection.AddSingleton<IEventSourceGrainSettingsMap>(settings);
            });

            builder.AddStartupTask<CallGrainStartupTask>();
            
            return builder;
        }
    }
}