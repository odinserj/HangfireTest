using System.Threading;

namespace Interface
{
    [Dispatcher("business/sleep")]
    public class Implementation : IDispatcher
    {
        public void Dispatch(object[] args)
        {
            Thread.Sleep(30000);
        }
    }
}
