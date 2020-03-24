using System.Threading.Tasks;
using Orleans;

namespace EventSourcing.Stream
{
    public interface IAggregateStream: IGrainWithStringKey
    {
        Task Ping();
    }
}