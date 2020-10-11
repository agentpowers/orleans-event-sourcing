﻿using Orleans;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

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
