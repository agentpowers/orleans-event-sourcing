using System;

namespace Events
{
    public class Event<T>
    {
        public T Data { get; set; }

        public Event(T data)
        {
            Data = data;
        }

        public Event(){}
    }
}