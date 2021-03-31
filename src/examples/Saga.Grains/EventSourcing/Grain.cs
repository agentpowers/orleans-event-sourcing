using System;
using System.Text.Json;
using System.Threading.Tasks;
using EventSourcingGrains.Grains;

namespace Saga.Grains.EventSourcing
{
    public abstract class SagaGrain<T> : EventSourceGrain<SagaState, ISagaEvent>, ISagaGrain<T> where T : class
    {
        public const string AggregateName = "saga";

        public SagaGrain() : base(AggregateName, new SagaAggregate()) { }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            // Hack: if state was just loaded from db, then serialize it to T and assign to context
            if (State.Context is JsonElement jsonElement)
            {
                State.Context = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
        }

        public Task Ping() => Task.CompletedTask;

        public async Task<SagaState> Execute(string id, T context)
        {
            if (State.Status == SagaStatus.NotStarted)
            {
                await ApplyEvent(new Executing { Id = id, Context = context });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Execute(T context)
        {
            if (State.Status == SagaStatus.Suspended || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new Executing { Id = State.Id, Context = context });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Executed()
        {
            if (State.Status == SagaStatus.Executing)
            {
                await ApplyEvent(new Executed { Id = State.Id });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Compensate(T context, string reason = null)
        {
            if (State.Status == SagaStatus.Suspended || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new Compensating { Id = State.Id, Context = context, Reason = reason });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Compensated()
        {
            if (State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Compensated { Id = State.Id });
                return State;
            }
            throw new Exception("Invalid Transition");

        }

        public async Task<SagaState> Cancel(string reason)
        {
            if (State.Status == SagaStatus.Suspended
                || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new Cancelled { Id = State.Id, Reason = reason });
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Fault(string error)
        {
            if (State.Status == SagaStatus.Executing
                || State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Faulted { Id = State.Id, Error = error });
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
                if (shouldSuspend)
                {
                    await ApplyEvents(new ISagaEvent[]
                    {
                        new StepExecuted{ Id = State.Id, Context = context },
                        new Suspended{ Id = State.Id },
                    });
                }
                else
                {
                    await ApplyEvent(new StepExecuted { Id = State.Id, Context = context });
                }
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> CompensateStep(T context, bool shouldSuspend = false)
        {
            if (State.Status == SagaStatus.Compensating)
            {
                if (shouldSuspend)
                {
                    await ApplyEvents(new ISagaEvent[]
                    {
                        new StepCompensated{ Id = State.Id, Context = context },
                        new Suspended{ Id = State.Id }
                    });
                }
                else
                {
                    await ApplyEvent(new StepCompensated { Id = State.Id, Context = context });
                }
                return State;
            }
            throw new Exception("Invalid Transition");
        }

        public async Task<SagaState> Suspend()
        {
            if (State.Status == SagaStatus.Executing || State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Suspended { Id = State.Id });
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