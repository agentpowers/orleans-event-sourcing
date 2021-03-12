using EventSourcing;

namespace Saga.Grains.EventSourcing
{
    public class SagaAggregate : IAggregate<SagaState, ISagaEvent>
    {
        public SagaState State { get; set; }
        public void Apply(ISagaEvent @event)
        {
            // set previous status
            State.PrevStatus = State.Status;
            // update status based on events
            switch (@event)
            {
                case Executing executing:
                    State.Context = executing.Context;
                    State.Status = SagaStatus.Executing;
                    break;
                case Executed _:
                    State.Status = SagaStatus.Executed;
                    break;
                case Compensating compensating:
                    State.CompensatingReason = compensating.Reason;
                    State.Status = SagaStatus.Compensating;
                    break;
                case Compensated _:
                    State.Status = SagaStatus.Compensated;
                    break;
                case Suspended _:
                    State.Status = SagaStatus.Suspended;
                    break;
                case Cancelled cancelled:
                    State.Status = SagaStatus.Cancelled;
                    State.CancelledReason = cancelled.Reason;
                    break;
                case Faulted faulted:
                    State.Status = SagaStatus.Faulted;
                    State.FaultedError = faulted.Error;
                    break;
                case StepExecuted stepExecuted:
                    State.Context = stepExecuted.Context;
                    State.SagaStepIndex++;
                    if (stepExecuted.ShouldSuspend)
                    {
                        State.Status = SagaStatus.Suspended;
                    }
                    break;
                case StepCompensated stepCompensated:
                    State.Context = stepCompensated.Context;
                    State.SagaStepIndex--;
                    if (stepCompensated.ShouldSuspend)
                    {
                        State.Status = SagaStatus.Suspended;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}