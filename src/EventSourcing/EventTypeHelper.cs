using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing
{
    public static class EventTypeHelper
    {
        private static readonly Dictionary<(string Name, int Version), Type> _eventIdentityToTypeCache = new Dictionary<(string Name, int Version), Type>();
        private static readonly Dictionary<Type, (string Name, int Version)> _typeToEventIdentityCache = new Dictionary<Type, (string Name, int Version)>();
        static EventTypeHelper()
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
            var baseEventType = typeof(IEvent);
            var attributeType = typeof(Event);
            foreach (var assembly in assemblyList)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (baseEventType.IsAssignableFrom(type))
                    {
                        var attribute = type.GetCustomAttributes(attributeType, false).FirstOrDefault();
                        if (attribute != default && attribute is Event eventMeta)
                        {
                            if (!_typeToEventIdentityCache.TryAdd(type, (eventMeta.Name, eventMeta.Version)))
                            {
                                throw new InvalidOperationException($"Unable to add type for {eventMeta}. Are there duplicate {nameof(Event)}?");
                            }
                            if (!_eventIdentityToTypeCache.TryAdd((eventMeta.Name, eventMeta.Version), type))
                            {
                                throw new InvalidOperationException($"Unable to add type for {eventMeta}. Are there duplicate {nameof(Event)}?");
                            }
                        }
                    }
                }
            }
        }
        // Get Type for Name and Version combo
        public static Type GetEventType((string Name, int Version) eventIdentity)
        {
            return _eventIdentityToTypeCache[eventIdentity];
        }

        // Get Name and Version for specific event type
        public static (string Name, int Version) GetEventIdentity(Type type)
        {
            return _typeToEventIdentityCache[type];
        }
    }
}
