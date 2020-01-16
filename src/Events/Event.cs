using System;

namespace Events
{
    public abstract class Event
    {
        public abstract string Type { get; set; }
    }
}