namespace EventSourcing
{
    public interface IEvent
    {
        string Type { get; set; }
    }

    public interface IState
    {
        void Init(string key);
    }
}