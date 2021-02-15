using System;
using System.Threading.Tasks;
using EventSourcingGrains.Grains;

namespace Saga.Grains.EventSourcing
{
    public abstract class SagaGrain<T> : EventSourceGrain<SagaState, ISagaEvent>, ISagaGrain<T>
    {
        public const string AggregateName = "saga";
        
        public SagaGrain() : base(AggregateName, new SagaAggregate()){}

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        public async Task<SagaState> Execute(string id, T context)
        {
            if (State.Status == SagaStatus.NotStarted)
            {
                await ApplyEvent(new ExecutingStarted{ Id = id, Context = context });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Execute(T context)
        {
            if (State.Status == SagaStatus.Suspended || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new ExecutingStarted{ Id = State.Id, Context = context });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Executed()
        {
            if (State.Status == SagaStatus.Executing)
            {
                await ApplyEvent(new Executed{ Id = State.Id });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Compensate(T context, string reason = null)
        {
            if (State.Status == SagaStatus.Suspended || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new CompensatingStarted{ Id = State.Id, Context = context, Reason = reason });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Compensated()
        {
            if (State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Compensated{ Id = State.Id });
                return State;
            }
            throw new Exception("Invalid Transition");

        }

        public async Task<SagaState> Cancel(string reason)
        {
            if (State.Status == SagaStatus.Suspended
                || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new Cancelled{ Id = State.Id, Reason = reason });
                return State;
            }
            throw new Exception("Invalid Transition");
        }
        
        public async Task<SagaState> Fault(string error)
        {
            if (State.Status == SagaStatus.Executing
                || State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Faulted{ Id = State.Id, Error = error });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public Task<SagaState> GetState()
        {
            return Task.FromResult(State);
        }

        public async Task<SagaState> ExecuteStep(T context, bool shouldSuspend = false)
        {
            if (State.Status == SagaStatus.Executing)
            {
                await ApplyEvent(new StepExecuted{ Id = State.Id, Context = context, ShouldSuspend = shouldSuspend });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> CompensateStep(T context, bool shouldSuspend = false)
        {
            if (State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new StepCompensated{ Id = State.Id, Context = context, ShouldSuspend = shouldSuspend });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Suspend()
        {
            if (State.Status == SagaStatus.Executing || State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Suspended{ Id = State.Id });
                return State;
            }
            throw new Exception("Invalid Transition");

        }

        // public Task<SagaState> ResumeExecution(T context)
        // {
        //     if (State.Status == SagaStatus.Suspended
        //         || State.Status == SagaStatus.Faulted)
        //     {
        //         await ApplyEvent(new Resumed{ Id = State.Id, Context = context });
        //         return State;
        //     }
        //     throw new Exception("Invalid Transition");
        // }

        // public async Task<SagaState> ResumeCompensating(T context)
        // {
        //     if (State.Status == SagaStatus.CompensatingSuspended
        //         || State.Status == SagaStatus.Faulted)
        //     {
        //         await ApplyEvent(new Resumed{ Id = State.Id, Context = context });
        //         return State;
        //     }
        //     throw new Exception("Invalid Transition");
        // }
    }
}