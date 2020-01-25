using System;

namespace EventSourcing
{
    public abstract class Event
    {
        public abstract string Type { get; set; }
    }
}