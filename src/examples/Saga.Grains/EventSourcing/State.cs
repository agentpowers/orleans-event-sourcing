using System;
using EventSourcing;

namespace Saga.Grains.EventSourcing
{
    [Flags]
    public enum SagaStatus
    {
        NotStarted = 0,
        Executing = 1,
        Executed = 2,
        Compensating = 3,
        Compensated = 4,
        Suspended = 5,
        Cancelled = 6,
        Faulted = 7
    }

    public class SagaState : IState
    {
        public SagaStatus Status { get; set; }
        public SagaStatus PrevStatus { get; set; }
        public string CancelledReason { get; set; }
        public string CompensatingReason { get; set; }
        public string FaultedError { get; set; }
        public int SagaStepIndex { get; set; }
        public object Context { get; set; }
        public string Id { get; set; }
        public void Init(string id)
        {
            Id = id;
        }
    }
}