using System;
using System.Collections.Generic;
using Orleans;

namespace EventSourcingGrains.Keeplive
{
    public class KeepAliveGrainSetting
    {
        public TimeSpan Interval { get; set; }
        public Func<IGrainFactory, IKeepAliveGrain> GrainResolver { get; set;}
    }
    public interface IKeepAliveSettings
    {
        List<KeepAliveGrainSetting> GrainKeepAliveSettings { get; }
    }

    public class KeepAliveSettings : IKeepAliveSettings
    {
        public List<KeepAliveGrainSetting> GrainKeepAliveSettings { get; private set;} = new List<KeepAliveGrainSetting>();
    }
}