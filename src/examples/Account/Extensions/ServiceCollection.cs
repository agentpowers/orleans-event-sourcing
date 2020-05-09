using Microsoft.Extensions.DependencyInjection;
using EventSourcingGrains.Extensions;
using Account.Grains.Repositories;

namespace Account.Extensions
{
    public static class ServiceCollection
    {
        public static void AddGrainServices(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IAccountRepository>(g => new AccountRepository(connectionString));
            services.AddEventSourcingGrain(connectionString);
        }
    }
}