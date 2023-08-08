using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Caching.Grains
{
    public interface ICacheGrain<T> : IGrainWithStringKey
    {
        Task Set(Immutable<T> value, TimeSpan delayDeactivation);
        Task<Immutable<T>> Get();
        Task Refresh();
        Task Clear();

    }
}
