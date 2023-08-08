using System;
using System.Collections.Generic;
using EventSourcing.Persistance;
using Orleans;

namespace EventSourcingGrains.Stream
{
    public class EventDispatcherSettings
    {
        public Func<AggregateEvent, IGrainFactory, IAggregateStreamReceiver> ReceiverGrainResolver { get; set; }
        // set to false by default which is less reliable.  If receiver can retrieve missed event in case of a failure then this can remain false
        public bool PersistDispatcherState { get; set; } = false;
        /// <summary>
        /// This value is used by Aggregate Stream Grain to determine if a dispatcher is under pressuer
        /// </summary>
        /// <value></value>
        public int QueueSizeThreshold { get; set; } = 1024 * 100;
    }
    public interface IAggregateStreamSettings
    {
        string AggregateName { get; }
        /// <summary>
        /// Maximum number of records to retrieved from db when fetching data
        /// </summary>
        /// <value></value>
        int QueryFetchSizeLimit { get; }
        bool ShouldHandleBackPressure { get; }
        Dictionary<string, EventDispatcherSettings> EventDispatcherSettingsMap { get; }
    }

    public class AggregateStreamSettings : IAggregateStreamSettings
    {
        public string AggregateName { get; private set; }

        public int QueryFetchSizeLimit { get; private set; }

        public bool ShouldHandleBackPressure { get; private set; }

        public Dictionary<string, EventDispatcherSettings> EventDispatcherSettingsMap { get; private set; }

        public AggregateStreamSettings(string aggregateName, int queryFetchSizeLimit = 1024 * 50, bool shouldHandleBackPressure = true)
        {
            AggregateName = aggregateName;
            QueryFetchSizeLimit = queryFetchSizeLimit;
            ShouldHandleBackPressure = shouldHandleBackPressure;
            EventDispatcherSettingsMap = new Dictionary<string, EventDispatcherSettings>();
        }
    }
}
