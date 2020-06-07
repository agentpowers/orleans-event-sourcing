using System.Collections.Generic;

namespace EventSourcingGrains.Grains
{
    public class EventSourceGrainSetting
    {
        public bool ShouldThrowIfAggregateDoesNotExist { get; set; }
    }

    public interface IEventSourceGrainSettingsMap: IDictionary<string, EventSourceGrainSetting>
    {
    }

    public class EventSourceGrainSettingsMap: Dictionary<string, EventSourceGrainSetting> , IEventSourceGrainSettingsMap
    {
    }
}