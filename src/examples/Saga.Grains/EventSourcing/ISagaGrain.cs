using System;
using System.Threading.Tasks;
using Orleans;

namespace Saga.Grains.EventSourcing
{
    // [Immutable]
    // public class SagaResponse
    // {
    //     public string SagaId { get; set; }
    //     public SagaStatus Status { get; set; }
    //     public string ErrorMessage { get; set; } = null;
    //     public string ErrorCode { get; set; } = null;
    //     public SagaResponse(string sagaId, SagaStatus status)
    //     {
    //         SagaId = sagaId;
    //         Status = status;
    //     }
    //     public SagaResponse(SagaState sagaState)
    //     {
    //         SagaId = sagaState.Id;
    //         Status = sagaState.Status;
    //     }
    //     public SagaResponse(SagaState sagaState, string errorMessage, string errorCode)
    //     {
    //         SagaId = sagaState.Id;
    //         Status = sagaState.Status;
    //         ErrorMessage = errorMessage;
    //         ErrorCode = errorCode;
    //     }
    //     public SagaResponse(string sagaId, SagaStatus status, string errorMessage, string errorCode)
    //     {
    //         SagaId = sagaId;
    //         Status = status;
    //         ErrorMessage = errorMessage;
    //         ErrorCode = errorCode;
    //     }
    // }

    public interface ISagaGrain<T> : IGrainWithStringKey
    {
        Task<SagaState> Execute(Guid id, T context);
        Task<SagaState> Execute(T context);
        Task<SagaState> Executed();
        Task<SagaState> Compensate(T context);
        Task<SagaState> Compensated();
        Task<SagaState> Suspend();
        Task<SagaState> Cancel(string reason);
        Task<SagaState> Fault(string error);
        Task<SagaState> ExecuteStep(T context, bool shouldSuspend = false);
        Task<SagaState> CompensateStep(T context, bool shouldSuspend = false);
        Task<SagaState> GetState();
    }
}