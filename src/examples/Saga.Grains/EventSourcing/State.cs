using System;
using EventSourcing;
using Orleans;

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
    [GenerateSerializer]
    public class SagaState : IState
    {
        [Id(0)]
        public SagaStatus Status { get; set; }
        [Id(1)]
        public SagaStatus PrevStatus { get; set; }
        [Id(2)]
        public string CancelledReason { get; set; }
        [Id(3)]
        public string CompensatingReason { get; set; }
        [Id(4)]
        public string FaultedError { get; set; }
        [Id(5)]
        public int SagaStepIndex { get; set; }
        [Id(6)]
        public object Context { get; set; }
        [Id(7)]
        public string Id { get; set; }
        public void Init(string id)
        {
            Id = id;
        }
    }
}
