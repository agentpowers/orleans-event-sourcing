using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace EventSourcing.Stream
{
    public static class ConfigureAggregateStreamExtensions
    {
        public static ISiloBuilder ConfigureAggregateStream(this ISiloBuilder builder, string aggregateName, Action<IAggregateStreamSettings> configureAggregateStream)
        {
            var aggregateStreamSettings = new AggregateStreamSettings(aggregateName);

            configureAggregateStream.Invoke(aggregateStreamSettings);

            builder.ConfigureServices((hostBuilder, serviceCollection) => 
            {
                serviceCollection.AddSingleton<IAggregateStreamSettings>(aggregateStreamSettings);
            });
            
            return builder;
        }
    }
}