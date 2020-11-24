using System.Collections.Concurrent;
using Interface;

namespace Hangfire.Server
{
    public sealed class DispatcherRegistry : IDispatcherRegistry
    {
        private readonly ConcurrentDictionary<string, IDispatcher> _dispatchers
            = new ConcurrentDictionary<string, IDispatcher>();

        public bool TryRegister(string message, IDispatcher dispatcher)
        {
            return _dispatchers.TryAdd(message, dispatcher);
        }

        public IDispatcher FindDispatcher(string message)
        {
            if (_dispatchers.TryGetValue(message, out var dispatcher))
            {
                return dispatcher;
            }

            return null;
        }
    }
}