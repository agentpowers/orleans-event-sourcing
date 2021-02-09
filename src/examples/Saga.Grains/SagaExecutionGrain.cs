using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Saga.Grains.EventSourcing
{
    public class SagaExecutionGrain<T>: SagaGrain<T>
    {
        private readonly ILogger<SagaExecutionGrain<T>> _logger;
        public readonly Type[] StepTypes;
        
        public SagaExecutionGrain(ILogger<SagaExecutionGrain<T>> logger)
        {
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            if (State.Status == SagaStatus.Executing || State.Status == SagaStatus.Compensating)
            {
                // start execution in background
                RegisterTimer(ExecuteSteps, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            }
        }

        public async Task<Guid> Start(T context)
        {
            var id = Guid.NewGuid();
            
            _logger.LogInformation("Message=Starting saga, SageId={0}", id);
            // change status to executing
            await Execute(id, context);
            // start execution in background
            RegisterTimer(ExecuteSteps, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            // return id
            return id;
        }

        public async Task Resume(T context)
        {   
            _logger.LogInformation("Message=Resuming saga, SageId={0}", State.Id);
            // change status to executing or compensating
            if (State.PrevStatus == SagaStatus.Executing)
            {
                await Execute(context);
            }
            else
            {
                await Compensate(context);
            }
            // start execution in background
            RegisterTimer(ExecuteSteps, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        //TODO: add retry
        //TODO: change status to faulted on error
        private async Task ExecuteSteps(object args)
        {
            if (State.Status == SagaStatus.Executing)
            {
                // execute steps
                while(State.SagaStepIndex < StepTypes.Length)
                {
                    var sagaStep = (ISagaStep<T>)ServiceProvider.GetService(StepTypes[State.SagaStepIndex]);
                    _logger.LogInformation("Message=Executing step, SageId={0}, Index={1}",State.Id, State.SagaStepIndex);
                    var context = await sagaStep.Execute(Context);
                    await ExecuteStep(context, sagaStep.ShouldSuspendAfterExecuting);
                    if (State.Status == SagaStatus.Suspended)
                    {
                        break;
                    }
                }
                // complete saga
                if (State.Status != SagaStatus.Suspended && State.SagaStepIndex == StepTypes.Length - 1)
                {
                    await Executed();
                    _logger.LogInformation("Message=Executed saga, SageId={0}, Index={1}",State.Id, State.SagaStepIndex);
                }
            }
            else if (State.Status == SagaStatus.Compensating)
            {
                // compensate steps
                while(State.SagaStepIndex >= 0)
                {
                    var sagaStep = (ISagaStep<T>)ServiceProvider.GetService(StepTypes[State.SagaStepIndex]);
                    _logger.LogInformation("Message=Compensating step, SageId={0}, Index={1}",State.Id, State.SagaStepIndex);
                    var context = await sagaStep.Execute(Context);
                    await CompensateStep(context, sagaStep.ShouldSuspendAfterCompensating);
                    if (State.Status == SagaStatus.Suspended)
                    {
                        break;
                    }
                }
                // complete saga
                if (State.Status != SagaStatus.Suspended && State.SagaStepIndex == 0)
                {
                    await Compensated();
                    _logger.LogInformation("Message=Compensated saga, SageId={0}, Index={1}",State.Id, State.SagaStepIndex);
                }
            }
        }

        private T Context => (T)State.Context;
    }
}