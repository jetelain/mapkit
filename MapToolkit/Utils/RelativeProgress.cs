using System;

namespace MapToolkit.Utils
{
    public class RelativeProgress : IProgress<double>
    {
        private readonly IProgress<double>? progress;
        private readonly double shift;
        private readonly double factor;

        public RelativeProgress(IProgress<double>? progress, double shift, double factor)
        {
            this.progress = progress;
            this.shift = shift;
            this.factor = factor;
        }

        public void Report(double value)
        {
            if (progress != null)
            {
                progress.Report(shift + value * factor);
            }
        }
    }
}
