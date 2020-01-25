using Microsoft.Extensions.DependencyInjection;
using EventSourcing.Persistance;

namespace EventSourcing.Extensions
{
    public static class Extensions
    {
        public static void AddEventSourcing(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IRepository>(g => new Repository(connectionString));
        }
    }
}