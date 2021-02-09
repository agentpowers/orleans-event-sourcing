using EventSourcing;

namespace Saga.Grains.EventSourcing
{
    public interface ISagaEvent: IEvent
    {
        string Id { get; set; }
    }

    public abstract class BaseSagaEvent: ISagaEvent
    {
        public string Id { get; set; }
    }

    #region Events
    [Event(nameof(ExecutingStarted))]
    public class ExecutingStarted : BaseSagaEvent
    {
        public object Context { get; set; }
    }

    [Event(nameof(Executed))]
    public class Executed : BaseSagaEvent
    {
    }

    [Event(nameof(CompensatingStarted))]
    public class CompensatingStarted : BaseSagaEvent
    {
        public object Context { get; set; }
    }

    [Event(nameof(Compensated))]
    public class Compensated : BaseSagaEvent
    {
    }

    [Event(nameof(Suspended))]
    public class Suspended : BaseSagaEvent
    {
    }

    [Event(nameof(Cancelled))]
    public class Cancelled : BaseSagaEvent
    {
        public string Reason { get; set; }
    }

    [Event(nameof(Faulted))]
    public class Faulted : BaseSagaEvent
    {
        public string Error { get; set; }
    }

    [Event(nameof(StepExecuted))]
    public class StepExecuted : BaseSagaEvent
    {
        public object Context { get; set; }
        public bool ShouldSuspend { get; set; }
    }

    [Event(nameof(StepCompensated))]
    public class StepCompensated : BaseSagaEvent
    {
        public object Context { get; set; }
        public bool ShouldSuspend { get; set; }
    }

    #endregion
}