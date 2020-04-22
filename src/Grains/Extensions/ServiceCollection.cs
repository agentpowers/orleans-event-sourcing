using Microsoft.Extensions.DependencyInjection;
using Grains.Repositories;

namespace Grains.Extensions
{
    public static class ServiceCollection
    {
        public static void AddGrainServices(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IAccountRepository>(g => new AccountRepository(connectionString));
        }
    }
}