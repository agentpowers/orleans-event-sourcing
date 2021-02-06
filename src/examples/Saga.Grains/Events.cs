using System.Collections.Generic;
using EventSourcing;

namespace Saga.Grains
{
    public interface ISagaEvent: IEvent
    {
        string Id { get; set; }
        Dictionary<string, string> Context { get; set; }
    }

    public abstract class BaseSagaEvent: ISagaEvent
    {
        public string Id { get; set; }
        public Dictionary<string, string> Context { get; set; }
    }

    #region Events
    [Event(nameof(Started))]
    public class Started : BaseSagaEvent
    {
    }

    [Event(nameof(Completed))]
    public class Completed : BaseSagaEvent
    {
    }

    [Event(nameof(Compensating))]
    public class Compensating : BaseSagaEvent
    {
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
    }

    [Event(nameof(Resumed))]
    public class Resumed : BaseSagaEvent
    {
    }

    [Event(nameof(Faulted))]
    public class Faulted : BaseSagaEvent
    {
    }

    #endregion
}