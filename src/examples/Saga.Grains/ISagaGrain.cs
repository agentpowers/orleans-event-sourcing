using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Saga.Grains
{
    [Immutable]
    public class SagaResponse
    {
        public string SagaId { get; set; }
        public SagaStatus Status { get; set; }
        public string ErrorMessage { get; set; } = null;
        public string ErrorCode { get; set; } = null;
        public SagaResponse(string sagaId, SagaStatus status)
        {
            SagaId = sagaId;
            Status = status;
        }
        public SagaResponse(SagaState sagaState)
        {
            SagaId = sagaState.Id;
            Status = sagaState.Status;
        }
        public SagaResponse(SagaState sagaState, string errorMessage, string errorCode)
        {
            SagaId = sagaState.Id;
            Status = sagaState.Status;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
        public SagaResponse(string sagaId, SagaStatus status, string errorMessage, string errorCode)
        {
            SagaId = sagaId;
            Status = status;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }
    }

    public interface ISagaGrain : IGrainWithStringKey
    {
        Task<SagaResponse> Execute();
        Task<SagaResponse> Complete();
        Task<SagaResponse> Compensate();
        Task<SagaResponse> Compensated();
        Task<SagaResponse> Suspend();
        Task<SagaResponse> Cancel();
        Task<SagaResponse> Resume();
        Task<SagaResponse> Fault();
        Task<SagaResponse> GetStatus();
    }
}