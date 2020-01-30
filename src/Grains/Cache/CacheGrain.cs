
using GrainInterfaces;
using System;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans;

namespace Grains.Cache
{
    public class CacheGrain<T> : Grain, ICacheGrain<T>
    {
        private Immutable<T> _state { get; set; } = new Immutable<T>(default(T));
        private TimeSpan _delayDeactivation { get; set; }

        public Task Clear()
        {
            // deactivate after this call
            DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public Task<Immutable<T>> Get()
        {
            // return state
            return Task.FromResult(_state);
        }

        public Task Refresh()
        {
            // delay deactivation
            DelayDeactivation(_delayDeactivation);
            return Task.CompletedTask;
        }

        public Task Set(Immutable<T> value, TimeSpan delayDeactivation)
        {
            // set delayDeactivation value
            _delayDeactivation = delayDeactivation;

            // set value
            _state = value;

            // delay deactivation
            DelayDeactivation(_delayDeactivation);

            return Task.CompletedTask;
        }
    }
}