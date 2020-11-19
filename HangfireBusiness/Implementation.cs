using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Interface
{
    public class Implementation : IEbmCoreService
    {
        public void DoWork()
        {
            Thread.Sleep(30000);
        }
    }
}
