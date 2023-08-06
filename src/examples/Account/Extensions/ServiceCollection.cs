using Account.Grains.Repositories;
using EventSourcingGrains.Extensions;
using Microsoft.Extensions.DependencyInjection;

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
