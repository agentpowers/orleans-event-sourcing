using Microsoft.Extensions.DependencyInjection;
using EventSourcing.Persistance;

namespace EventSourcing.Extensions
{
    public static class Extensions
    {
        public static void AddEventSourcing(this IServiceCollection services)
        {
            services.AddTransient<IRepository, Repository>();
        }
    }
}