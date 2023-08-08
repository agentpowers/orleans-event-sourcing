using System.Text.Json;
using EventSourcing.Persistance;

namespace EventSourcing
{
    public static class EventSerializer
    {
        // deserialize from db event
        public static object DeserializeEvent(AggregateEventBase dbEvent)
        {
            var eventType = EventTypeHelper.GetEventType((dbEvent.Type, dbEvent.EventVersion));
            return JsonSerializer.Deserialize(dbEvent.Data, eventType);
        }
    }
}
