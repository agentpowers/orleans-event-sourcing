using System.Threading.Tasks;
using Orleans;

namespace EventSourcingGrains.Keeplive
{
    public interface IKeepAliveGrain : IGrainWithStringKey
    {
        Task Ping();
    }
}