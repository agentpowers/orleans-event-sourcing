using Microsoft.Extensions.DependencyInjection;
using EventSourcing.Persistance;

namespace EventSourcing.Extensions
{
    public static class ServiceCollection
    {
        public static void AddEventSourcing(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IRepository>(g => new Repository(connectionString));
            services.AddTransient(typeof(IEventSource<,>), typeof(EventSource<,>));
        }
    }
}