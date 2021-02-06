using EventSourcing;

namespace Saga.Grains
{
    public class SagaAggregate : IAggregate<SagaState, ISagaEvent>
    {
        public SagaState State { get; set; }
        public void Apply(ISagaEvent @event)
        {
            switch (@event)
            {
                case Started started:
                    State.Status = SagaStatus.Executing;
                    State.Context = started.Context;
                    break;
                case Completed completed:
                    State.Status = SagaStatus.Completed;
                    State.Context = completed.Context;
                    break;
                case Compensating compensating:
                    State.Status = SagaStatus.Compensating;
                    State.Context = compensating.Context;
                    break;
                case Compensated compensated:
                    State.Status = SagaStatus.Compensated;
                    State.Context = compensated.Context;
                    break;
                case Suspended suspended:
                    State.Status = SagaStatus.Suspended;
                    State.Context = suspended.Context;
                    break;
                case Cancelled cancelled:
                    State.Status = SagaStatus.Cancelled;
                    State.Context = cancelled.Context;
                    break;
                case Resumed resumed:
                    State.Status = SagaStatus.Executing;
                    State.Context = resumed.Context;
                    break;
                case Faulted faulted:
                    State.Status = SagaStatus.Faulted;
                    State.Context = faulted.Context;
                    break;
                default:
                    break;
            }
        }
    }
}