using EventSourcing.Grains;
using Newtonsoft.Json;

namespace EventSourcing
{
    public static class JsonSerializer
    {       
        // serialize an event by wrapping that using EventWrapper and then uses JSON.NET typenamehandling
        public static string SerializeEvent(IEvent obj)
        {
            return JsonConvert.SerializeObject(new EventWrapper{ Event = obj });
        }

        // deserialize json to event(json is wrapped using EventWrapper)
        public static TEvent DeserializeEvent<TEvent>(string json) where TEvent : IEvent
        {
            var eventWrapper =  JsonConvert.DeserializeObject<EventWrapper>(json);
            return (TEvent)eventWrapper.Event;
        }       
    }
}
