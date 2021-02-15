using System;
using System.Threading.Tasks;
using Orleans;
using Saga.Grains.EventSourcing;

namespace Saga.Grains
{
    public class StepOne : ISagaStep<int>
    {
        public bool ShouldSuspendAfterExecuting { get;set; }
        public bool ShouldSuspendAfterCompensating { get;set; }

        public async Task<int> Compensate(int state)
        {
            await Task.Delay(100);
            return --state;
        }

        public async Task<int> Execute(int state)
        {
            await Task.Delay(100);
            return ++state;
        }
    }
    public class StepTwo : ISagaStep<int>
    {
        public bool ShouldSuspendAfterExecuting { get;set; } = true;
        public bool ShouldSuspendAfterCompensating { get;set; } = true;

        public async Task<int> Compensate(int state)
        {
            await Task.Delay(100);
            return --state;
        }

        public async Task<int> Execute(int state)
        {
            await Task.Delay(100);
            return ++state;
        }
    }
    public class StepThree : ISagaStep<int>
    {
        public bool ShouldSuspendAfterExecuting { get;set; }
        public bool ShouldSuspendAfterCompensating { get;set; }

        public async Task<int> Compensate(int state)
        {
            await Task.Delay(100);
            return --state;
        }

        public async Task<int> Execute(int state)
        {
            await Task.Delay(100);
            return ++state;
        }
    }
    public interface ITestSaga: IGrainWithStringKey
    {
        Task<string> Start(int context);
        Task<(SagaStatus, int)> GetStatus();
        Task Resume(int context);
        Task Revert(string reason);
    }
    public class TestSaga: SagaExecutionGrain<int>, ITestSaga
    {
        public override Type[] StepTypes { get; set; }

        public TestSaga()
        {
            var stepBuilder = new SagaExecutionStepsBuilder<int>()
                                .AddStep<StepOne>()
                                .AddStep<StepTwo>()
                                .AddStep<StepThree>();
            StepTypes = stepBuilder.Build();
        }

        public Task<(SagaStatus, int)> GetStatus()
        {
            return Task.FromResult((State.Status, Context));
        }
    }
}