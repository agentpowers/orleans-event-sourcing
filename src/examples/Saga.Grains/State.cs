using System;
using System.Collections.Generic;
using EventSourcing;

namespace Saga.Grains
{
    [Flags]
    public enum SagaStatus
    {
        NotStarted = 0,
        Executing = 1,
        Completed = 2,
        Compensating = 3,
        Compensated = 4,
        Suspended = 5,
        Cancelled = 6,
        Faulted = 7
    }
    
    public class SagaState : IState
    {
        public SagaStatus Status { get; set; }
        public Dictionary<string, string> Context { get; set; }
        public string Id { get; set; }
        public void Init(string id)
        {
            Id = id;
        }
    }
}