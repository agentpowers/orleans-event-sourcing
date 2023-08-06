using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saga.Grains
{
    public interface ISagaStep<T>
    {
        Task<T> Execute(T state);
        Task<T> Compensate(T state);
        bool ShouldSuspendAfterExecuting { get; set; }
        bool ShouldSuspendAfterCompensating { get; set; }
    }

    public class SagaExecutionStepsBuilder<T>
    {
        private readonly List<Type> _sagaStepTypes = null;
        public SagaExecutionStepsBuilder()
        {
            _sagaStepTypes = new List<Type>();
        }

        public SagaExecutionStepsBuilder<T> AddStep<TStep>() where TStep : ISagaStep<T>
        {
            _sagaStepTypes.Add(typeof(TStep));
            return this;
        }

        public Type[] Build()
        {
            return _sagaStepTypes.ToArray();
        }
    }
}
