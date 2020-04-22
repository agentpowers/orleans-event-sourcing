using System;
using System.Collections.Generic;
using EventSourcing.Persistance;
using Orleans;

namespace EventSourcing.Stream
{
    public interface IAggregateStreamSettings
    {
        string AggregateName {get;}
        Dictionary<string, Func<AggregateEvent, IGrainFactory, IAggregateStreamReceiver>> EventReceiverGrainResolverMap { get; }
    }

    public class AggregateStreamSettings : IAggregateStreamSettings
    {
        public string AggregateName { get; private set;}

        public Dictionary<string, Func<Persistance.AggregateEvent, IGrainFactory, IAggregateStreamReceiver>> EventReceiverGrainResolverMap { get; private set; }

        public AggregateStreamSettings(string aggregateName)
        {
            AggregateName = aggregateName;
            EventReceiverGrainResolverMap = new Dictionary<string, Func<Persistance.AggregateEvent, IGrainFactory, IAggregateStreamReceiver>>();
        }
    }
}