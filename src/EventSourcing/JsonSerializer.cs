using System;
using EventSourcing.Grains;
using Newtonsoft.Json;

namespace EventSourcing
{
    public static class JsonSerializer
    {       
        // serialize an event by wrapping that using EventWrapper and then uses JSON.NET typenamehandling
        public static string SerializeEvent(Event obj)
        {
            return JsonConvert.SerializeObject(new EventWrapper{ Event = obj });
        }

        // deserialize json to event
        public static TEvent DeserializeEvent<TEvent>(string json) where TEvent : Event
        {
            var eventWrapper =  JsonConvert.DeserializeObject<EventWrapper>(json);
            return eventWrapper.Event as TEvent;
        }       
    }
}
