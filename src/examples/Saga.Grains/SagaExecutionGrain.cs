using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Saga.Grains.EventSourcing
{
    public abstract class SagaExecutionGrain<T> : SagaGrain<T> where T : class
    {
        private ILogger<SagaExecutionGrain<T>> _logger;
        private readonly TimeSpan _expiration = TimeSpan.FromMinutes(5);
        private IDisposable _timer;
        public abstract Type[] StepTypes { get; set; }

        protected SagaExecutionGrain()
        {
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken = default)
        {
            _logger = ServiceProvider.GetService<ILogger<SagaExecutionGrain<T>>>();
            await base.OnActivateAsync();
            if (State.Status == SagaStatus.Executing || State.Status == SagaStatus.Compensating)
            {
                // start execution in background
                _timer = RegisterTimer(ExecuteSteps, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            }
        }

        public async Task<string> Start(T context)
        {
            var id = GetGrainKey();

            _logger.LogInformation("Message=Starting saga, SageId={0}", id);
            // change status to executing
            await Execute(id, context);
            // start execution in background
            _timer = RegisterTimer(ExecuteSteps, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
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
            _timer = RegisterTimer(ExecuteSteps, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async Task Revert(string reason)
        {
            var id = GetGrainKey();

            _logger.LogInformation("Message=Reverting saga, SageId={0}", id);
            // change status to executing
            await Compensate(Context, reason);
            // start execution in background
            _timer = RegisterTimer(ExecuteSteps, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        //TODO: add retry
        //TODO: change status to faulted on error
        private async Task ExecuteSteps(object args)
        {
            // dispose timer
            _timer.Dispose();
            if (State.Status == SagaStatus.Executing)
            {
                // execute steps
                while (State.SagaStepIndex < StepTypes.Length)
                {
                    var sagaStep = (ISagaStep<T>)ServiceProvider.GetService(StepTypes[State.SagaStepIndex]);
                    if (sagaStep == null)
                    {
                        throw new InvalidOperationException($"Message=unable to retrieve service for step, StepType={StepTypes[State.SagaStepIndex]}");
                    }

                    _logger.LogInformation("Message=Executing step, SageId={0}, Index={1}", State.Id, State.SagaStepIndex);
                    var context = await sagaStep.Execute(Context);
                    await ExecuteStep(context, sagaStep.ShouldSuspendAfterExecuting);
                    if (State.Status == SagaStatus.Suspended)
                    {
                        break;
                    }
                }
                // complete saga
                if (State.Status != SagaStatus.Suspended && State.SagaStepIndex == StepTypes.Length)
                {
                    await Executed();
                    _logger.LogInformation("Message=Executed saga, SageId={0}, Index={1}", State.Id, State.SagaStepIndex);
                    // set grain expiration
                    DelayDeactivation(_expiration);
                }
            }
            else if (State.Status == SagaStatus.Compensating)
            {
                // compensate steps
                while (State.SagaStepIndex >= 0)
                {
                    var sagaStep = (ISagaStep<T>)ServiceProvider.GetService(StepTypes[State.SagaStepIndex]);
                    _logger.LogInformation("Message=Compensating step, SageId={0}, Index={1}", State.Id, State.SagaStepIndex);
                    var context = await sagaStep.Execute(Context);
                    await CompensateStep(context, sagaStep.ShouldSuspendAfterCompensating);
                    if (State.Status == SagaStatus.Suspended)
                    {
                        break;
                    }
                }
                // complete saga
                if (State.Status != SagaStatus.Suspended && State.SagaStepIndex == -1)
                {
                    await Compensated();
                    _logger.LogInformation("Message=Compensated saga, SageId={0}, Index={1}", State.Id, State.SagaStepIndex);
                    // set grain expiration
                    DelayDeactivation(_expiration);
                }
            }
        }

        protected T Context => (T)State.Context;
    }
}
