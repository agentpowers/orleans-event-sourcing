using System.Threading.Tasks;
using EventSourcing.Keeplive;
using Orleans;

namespace EventSourcing.Stream
{
    public interface IAggregateStreamGrain: IKeepAliveGrain, IGrainWithStringKey
    {
    }
}