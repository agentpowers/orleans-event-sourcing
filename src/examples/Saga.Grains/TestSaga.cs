using System;
using System.Threading.Tasks;
using Orleans;
using Saga.Grains.EventSourcing;

namespace Saga.Grains
{
    public class TestSagaState
    {
        public int Value { get; set; }
    }
    public class StepOne : ISagaStep<TestSagaState>
    {
        public bool ShouldSuspendAfterExecuting { get; set; }
        public bool ShouldSuspendAfterCompensating { get; set; }

        public async Task<TestSagaState> Compensate(TestSagaState state)
        {
            await Task.Delay(100);
            --state.Value;
            return state;
        }

        public async Task<TestSagaState> Execute(TestSagaState state)
        {
            await Task.Delay(100);
            ++state.Value;
            return state;
        }
    }
    public class StepTwo : ISagaStep<TestSagaState>
    {
        public bool ShouldSuspendAfterExecuting { get; set; } = true;
        public bool ShouldSuspendAfterCompensating { get; set; } = true;

        public async Task<TestSagaState> Compensate(TestSagaState state)
        {
            await Task.Delay(100);
            --state.Value;
            return state;
        }

        public async Task<TestSagaState> Execute(TestSagaState state)
        {
            await Task.Delay(100);
            ++state.Value;
            return state;
        }
    }
    public class StepThree : ISagaStep<TestSagaState>
    {
        public bool ShouldSuspendAfterExecuting { get; set; }
        public bool ShouldSuspendAfterCompensating { get; set; }

        public async Task<TestSagaState> Compensate(TestSagaState state)
        {
            await Task.Delay(100);
            --state.Value;
            return state;
        }

        public async Task<TestSagaState> Execute(TestSagaState state)
        {
            await Task.Delay(100);
            --state.Value;
            return state;
        }
    }
    public class StepFour : ISagaStep<TestSagaState>
    {
        public bool ShouldSuspendAfterExecuting { get; set; }
        public bool ShouldSuspendAfterCompensating { get; set; }

        public async Task<TestSagaState> Compensate(TestSagaState state)
        {
            await Task.Delay(TimeSpan.FromMinutes(20));
            --state.Value;
            return state;
        }

        public async Task<TestSagaState> Execute(TestSagaState state)
        {
            await Task.Delay(TimeSpan.FromMinutes(20));
            ++state.Value;
            return state;
        }
    }
    public interface ITestSaga : IGrainWithStringKey
    {
        Task<string> Start(TestSagaState context);
        Task<SagaState> GetState();
        Task Resume(TestSagaState context);
        Task Revert(string reason);
    }
    public class TestSaga : SagaExecutionGrain<TestSagaState>, ITestSaga
    {
        public override Type[] StepTypes { get; set; }

        public TestSaga()
        {
            var stepBuilder = new SagaExecutionStepsBuilder<TestSagaState>()
                                .AddStep<StepOne>()
                                .AddStep<StepTwo>()
                                .AddStep<StepThree>()
                                .AddStep<StepFour>();
            StepTypes = stepBuilder.Build();
        }
    }
}