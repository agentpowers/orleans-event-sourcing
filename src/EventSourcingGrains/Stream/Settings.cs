using System;
using System.Collections.Generic;
using EventSourcing.Persistance;
using Orleans;

namespace EventSourcingGrains.Stream
{
    public class EventDispatcherSettings
    {
        public Func<AggregateEvent, IGrainFactory, IAggregateStreamReceiver> ReceiverGrainResolver { get; set;}
        // set to false by default which is less reliable.  If receiver can retrieve missed event in case of a failure then this can remain false
        public bool PersistDispatcherState { get; set; } = false;
    }
    public interface IAggregateStreamSettings
    {
        string AggregateName {get;}
        Dictionary<string, EventDispatcherSettings> EventDispatcherSettingsMap { get; }
    }

    public class AggregateStreamSettings : IAggregateStreamSettings
    {
        public string AggregateName { get; private set;}

        public Dictionary<string, EventDispatcherSettings> EventDispatcherSettingsMap { get; private set; }

        public AggregateStreamSettings(string aggregateName)
        {
            AggregateName = aggregateName;
            EventDispatcherSettingsMap = new Dictionary<string, EventDispatcherSettings>();
        }
    }
}