namespace EventSourcing
{
    public interface IState
    {
        void Init(string key);
    }
}
