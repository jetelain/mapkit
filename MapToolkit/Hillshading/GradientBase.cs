using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDEM.Hillshading
{
    public abstract class GradientBase 
    {
        private readonly double xFactor;
        private readonly double yFactor;
        private readonly bool isSameFactor;

        /// <summary>
        /// Initialize common parameters.
        /// </summary>
        /// <param name="resolution">Resolution of a pixel in meters</param>
        /// <param name="altFactor">Elevation exageration factor</param>
        /// <param name="points">Number of points per sample for current algorithm</param>
        private protected GradientBase(Vector resolution, double altFactor, int points)
        {
            xFactor = altFactor / points / resolution.X;
            yFactor = altFactor / points / resolution.Y;
            isSameFactor = resolution.X == resolution.Y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="southLine"></param>
        /// <param name="line"></param>
        /// <param name="northLine"></param>
        /// <param name="x"></param>
        /// <param name="dx">East - West elevation delta in meters</param>
        /// <param name="dy">South - North elevation delta in meters</param>
        internal abstract void GetDeltaUnscaled(double[] southLine, double[] line, double[] northLine, int x, out double dx, out double dy);

        internal void GetSlopeAndAspect(double[] southLine, double[] line, double[] northLine, int x, out double slope, out double aspect)
        {
            if (isSameFactor)
            {
                GetDeltaUnscaled(southLine, line, northLine, x, out var dx, out var dy);
                slope = Math.Atan(Math.Sqrt(dx * dx + dy * dy) * xFactor);
                aspect = Math.Atan2(dy, -dx);
            }
            else
            {
                GetDelta(southLine, line, northLine, x, out var dx, out var dy);
                slope = Math.Atan(Math.Sqrt(dx * dx + dy * dy));
                aspect = Math.Atan2(dy, -dx);
            }
        }

        internal void GetDelta(double[] southLine, double[] line, double[] northLine, int x, out double sdx, out double sdy)
        {
            GetDeltaUnscaled(southLine, line, northLine, x, out var dx, out var dy);
            sdx = dx * xFactor;
            sdy = dy * yFactor;
        }
    }
}
