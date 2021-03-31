using Microsoft.Extensions.DependencyInjection;
using EventSourcingGrains.Extensions;
using Saga.Grains.Extensions;

namespace SagaExample.Extensions
{
    public static class ServiceCollection
    {
        public static void AddGrainServices(this IServiceCollection services, string connectionString)
        {
            services.AddEventSourcingGrain(connectionString);

            // add saga steps
            services.AddSagaSteps();
        }

        public static void AddInMemoryGrainServices(this IServiceCollection services)
        {
            services.AddInMemoryEventSourcingGrain();

            // add saga steps
            services.AddSagaSteps();
        }
    }
}