using System;

namespace EventSourcing
{
    public abstract class Event
    {
        public abstract string Type { get; set; }
    }

    public abstract class State
    {
        public abstract void Init(string id);
    }
}