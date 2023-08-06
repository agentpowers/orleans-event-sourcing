using EventSourcing.Persistance;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Extensions
{
    public static class ServiceCollection
    {
        public static void AddEventSourcing(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IRepository>(g => new PostgresRepository(connectionString));
            services.AddTransient(typeof(IEventSource<,>), typeof(EventSource<,>));
        }

        public static void AddInMemoryEventSourcing(this IServiceCollection services)
        {
            services.AddTransient<IRepository, InMemoryRepository>();
            services.AddTransient(typeof(IEventSource<,>), typeof(EventSource<,>));
        }
    }
}
