using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDEM.Hillshading
{
    public abstract class GradientBase 
    {
        // XXX: Assume square pixels

        private readonly double factorScaled;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factor">Elevation factor</param>
        /// <param name="points">Number of points per sample</param>
        private protected GradientBase(double factor, int points)
        {
            factorScaled = factor / points;
        }

        internal abstract void GetDeltaUnscaled(double[] southLine, double[] line, double[] northLine, int x, out double dx, out double dy);

        internal void GetSlopeAndAspect(double[] southLine, double[] line, double[] northLine, int x, out double slope, out double aspect)
        {
            GetDeltaUnscaled(southLine, line, northLine, x, out var dx, out var dy);
            
            slope = Math.Atan(Math.Sqrt(dx * dx + dy * dy) * factorScaled); 
            aspect = Math.Atan2(dy, -dx);
        }

        internal void GetDelta(double[] southLine, double[] line, double[] northLine, int x, out double sdx, out double sdy)
        {
            GetDeltaUnscaled(southLine, line, northLine, x, out var dx, out var dy);
            sdx = dx * factorScaled;
            sdy = dy * factorScaled;
        }
    }
}
