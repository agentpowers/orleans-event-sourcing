using System.Threading.Tasks;
using EventSourcing.Persistance;
using Orleans;
using Orleans.Concurrency;

namespace EventSourcingGrains.Stream
{
    public interface IAggregateStreamDispatcherGrain : IGrainWithStringKey
    {
        // Notify aggregate stream dispatcher grain about new events
        ValueTask<bool> AddToQueue(Immutable<AggregateEvent[]> events);
        ValueTask<bool> IsUnderPressure();
        ValueTask<long> GetLastQueuedEventId();
    }
}