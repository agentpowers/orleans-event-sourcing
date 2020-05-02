using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;

namespace EventSourcingGrains.Keeplive
{
    public static class ConfigureKeepAliveSettingsExtensions
    {
        public static ISiloBuilder ConfigureKeepAliveService(this ISiloBuilder builder, Action<IKeepAliveSettings> configure)
        {
            var keepAliveSettings = new KeepAliveSettings();

            configure.Invoke(keepAliveSettings);

            builder.ConfigureServices((hostBuilder, serviceCollection) => 
            {
                serviceCollection.AddSingleton<IKeepAliveSettings>(keepAliveSettings);
            });
            
            return builder;
        }
    }
}