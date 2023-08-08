using Microsoft.Extensions.DependencyInjection;

namespace Saga.Grains.Extensions
{
    public static class ServiceCollection
    {
        public static void AddSagaSteps(this IServiceCollection services)
        {
            services.AddTransient<StepOne>();
            services.AddTransient<StepTwo>();
            services.AddTransient<StepThree>();
            services.AddTransient<StepFour>();
        }
    }
}
