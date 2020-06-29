using System.Threading.Tasks;
using EventSourcingGrains.Keeplive;
using Orleans;

namespace EventSourcingGrains.Stream
{
    public interface IAggregateStreamGrain: IKeepAliveGrain, IGrainWithStringKey
    {
        // Notify aggregate stream grain about new event
        Task Notify(long eventId);
    }
}