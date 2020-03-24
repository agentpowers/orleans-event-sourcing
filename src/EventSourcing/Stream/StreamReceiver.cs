using System.Threading.Tasks;
using EventSourcing.Persistance;
using Orleans;

namespace EventSourcing.Stream
{
    public interface IAggregateStreamReceiver : IGrain
    {
        Task Receive(AggregateEvent @event);
    }
}