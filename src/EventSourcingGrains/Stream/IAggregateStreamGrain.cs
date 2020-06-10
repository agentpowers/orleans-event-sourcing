using System.Threading.Tasks;
using EventSourcingGrains.Keeplive;
using Orleans;
using Orleans.Concurrency;

namespace EventSourcingGrains.Stream
{
    public interface IAggregateStreamGrain: IKeepAliveGrain, IGrainWithStringKey
    {
        // Notify aggregate stream grain about new event
        [OneWay]
        Task Notify(long eventId);
    }
}