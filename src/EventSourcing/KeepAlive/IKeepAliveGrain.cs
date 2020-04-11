using System.Threading.Tasks;
using Orleans;

namespace EventSourcing.Keeplive
{
    public interface IKeepAliveGrain: IGrainWithStringKey
    {
        Task Ping();
    }
}