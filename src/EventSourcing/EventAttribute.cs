namespace EventSourcing
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class Event : System.Attribute
    {
        public string Name;
        public int Version = 1;

        public Event(string name) => Name = name;
        public Event(string name, int version)
        {
            Name = name;
            Version = version;
        }

        public override string ToString()
        {
            return $"{Name}:{Version}";
        }
    }
}