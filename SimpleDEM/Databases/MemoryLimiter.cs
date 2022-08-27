using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SimpleDEM.Databases
{
    internal class MemoryLimiter
    {
        private readonly long maxBytes;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private long loadedBytes = 0;
    }
}
