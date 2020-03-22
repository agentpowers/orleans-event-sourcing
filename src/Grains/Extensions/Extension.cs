using Microsoft.Extensions.DependencyInjection;
using EventSourcing.Persistance;
using Grains.Repositories;
using Orleans.Hosting;
using EventSourcing.Stream;
using Grains.Account;

namespace Grains
{
    public static class Extensions
    {
        public static void AddGrainServices(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IAccountRepository>(g => new AccountRepository(connectionString));
        }

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