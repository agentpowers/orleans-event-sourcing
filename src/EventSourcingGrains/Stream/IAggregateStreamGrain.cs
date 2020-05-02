using EventSourcingGrains.Keeplive;
using Orleans;

namespace EventSourcingGrains.Stream
{
    public interface IAggregateStreamGrain: IKeepAliveGrain, IGrainWithStringKey
    {
    }
}