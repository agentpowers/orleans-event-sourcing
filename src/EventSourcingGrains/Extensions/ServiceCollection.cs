using EventSourcing.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingGrains.Extensions
{
    public static class ServiceCollection
    {
        public static void AddEventSourcingGrain(this IServiceCollection services, string connectionString)
        {
            services.AddEventSourcing(connectionString);
        }

        public static void AddInMemoryEventSourcingGrain(this IServiceCollection services)
        {
            services.AddInMemoryEventSourcing();
        }
    }
}
