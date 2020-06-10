using System.Threading.Tasks;
using EventSourcing.Persistance;
using Orleans;
using Orleans.Concurrency;

namespace EventSourcingGrains.Stream
{
    public interface IAggregateStreamDispatcherGrain: IGrainWithStringKey
    {
        // Notify aggregate stream dispatcher grain about new events
        Task AddToQueue(Immutable<AggregateEvent[]> events);
        ValueTask<long> GetLastQueuedEventId();
    }
}