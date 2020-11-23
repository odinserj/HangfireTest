namespace Interface
{
    public interface IDispatcherRegistry
    {
        IDispatcher FindDispatcher(string message);
    }
}