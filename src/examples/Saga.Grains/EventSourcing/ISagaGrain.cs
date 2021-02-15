using System;
using System.Threading.Tasks;
using Orleans;

namespace Saga.Grains.EventSourcing
{

    public interface ISagaGrain<T> : IGrainWithStringKey
    {
        Task<SagaState> Execute(string id, T context);
        Task<SagaState> Execute(T context);
        Task<SagaState> Executed();
        Task<SagaState> Compensate(T context, string reason = null);
        Task<SagaState> Compensated();
        Task<SagaState> Suspend();
        Task<SagaState> Cancel(string reason);
        Task<SagaState> Fault(string error);
        Task<SagaState> ExecuteStep(T context, bool shouldSuspend = false);
        Task<SagaState> CompensateStep(T context, bool shouldSuspend = false);
        Task<SagaState> GetState();
    }
}