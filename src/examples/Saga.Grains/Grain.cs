using System.Threading.Tasks;
using EventSourcingGrains.Grains;
using Microsoft.Extensions.Logging;

namespace Saga.Grains
{
    public class SagaGrain : EventSourceGrain<SagaState, ISagaEvent>, ISagaGrain
    {
        public const string AggregateName = "saga";
        private readonly ILogger<SagaGrain> _logger;
        
        public SagaGrain(ILogger<SagaGrain> logger) : base(AggregateName, new SagaAggregate())
        {
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        public async Task<SagaResponse> Execute()
        {
            var key = GetGrainKey();
            if (State.Status == SagaStatus.NotStarted)
            {
                await ApplyEvent(new Started{ Id = key, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(key, State.Status, "Invalid Transition", "I");
        }

        public async Task<SagaResponse> Complete()
        {
            var key = GetGrainKey();
            if (State.Status == SagaStatus.Executing)
            {
                await ApplyEvent(new Completed{ Id = key, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(key, State.Status, "Invalid Transition", "I");
        }

        public async Task<SagaResponse> Compensate()
        {
            if (State.Status == SagaStatus.Suspended
                || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new Compensating{ Id = State.Id, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(State, "Invalid Transition", "I");

        }

        public async Task<SagaResponse> Compensated()
        {
            if (State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Compensated{ Id = State.Id, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(State, "Invalid Transition", "I");

        }

        public async Task<SagaResponse> Suspend()
        {
            if (State.Status == SagaStatus.Executing)
            {
                await ApplyEvent(new Suspended{ Id = State.Id, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(State, "Invalid Transition", "I");

        }

        public async Task<SagaResponse> Cancel()
        {
            if (State.Status == SagaStatus.Suspended
                || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new Cancelled{ Id = State.Id, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(State, "Invalid Transition", "I");
        }

        public async Task<SagaResponse> Resume()
        {
            if (State.Status == SagaStatus.Suspended
                || State.Status == SagaStatus.Faulted)
            {
                await ApplyEvent(new Resumed{ Id = State.Id, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(State, "Invalid Transition", "I");
        }
        
        public async Task<SagaResponse> Fault()
        {
            if (State.Status == SagaStatus.Executing
                || State.Status == SagaStatus.Compensating)
            {
                await ApplyEvent(new Faulted{ Id = State.Id, Context = State.Context });
                return new SagaResponse(State);
            }
            return new SagaResponse(State, "Invalid Transition", "I");
        }

        public Task<SagaResponse> GetStatus()
        {
            return Task.FromResult(new SagaResponse(State));
        }
    }
}