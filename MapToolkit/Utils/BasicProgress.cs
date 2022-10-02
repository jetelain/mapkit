using System;
using System.Threading;

namespace MapToolkit.Utils
{
    public sealed class BasicProgress : IDisposable
    {
        private readonly IProgress<double>? progress;
        private readonly int total;
        private readonly int step;
        private int done;

        public BasicProgress(IProgress<double>? progress, int total) 
            : this( progress, total, total / 100)
        {

        }

        public BasicProgress(IProgress<double>? progress, int total, int step)
        {
            this.progress = progress;
            this.total = total;
            this.step = Math.Max(1, step);
        }

        public void AddOne()
        {
            if (progress != null && Interlocked.Increment(ref done) % step == 0)
            {
                progress.Report(done * 100.0 / total);
            }
        }

        public void Add(int add)
        {
            if (progress != null && Interlocked.Add(ref done, add) % step == 0 && total != 0)
            {
                progress.Report(done * 100.0 / total);
            }
        }

        public void Dispose()
        {
            if (progress != null)
            {
                progress.Report(100.0);
            }
        }
    }
}
