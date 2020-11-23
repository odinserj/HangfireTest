using System;
using System.ComponentModel;

namespace Interface
{
    public class EbmCoreService
    {
        private readonly IDispatcherRegistry _registry;

        public EbmCoreService(IDispatcherRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        [DisplayName("EBM ({0})")]
        public void Execute(string message, object[] args)
        {
            var dispatcher = _registry.FindDispatcher(message);
            dispatcher.Dispatch(args);
        }
    }
}
