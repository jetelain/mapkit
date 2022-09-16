using System;

namespace MapToolkit.Hillshading
{
    public sealed class Horn : GradientBase
    {
        public Horn(Vector resolution, double factor = 1) 
            : base(resolution, factor, 8)
        {

        }

        internal override void GetDeltaUnscaled(double[] southLine, double[] line, double[] northLine, int x, out double dx, out double dy)
        {
            var wx = Math.Max(0, x - 1);
            var ex = Math.Min(line.Length - 1, x + 1);

            var nw = northLine[wx];
            var n = northLine[x];
            var ne = northLine[ex];
            var w = line[wx];
            var e = line[ex];
            var sw = southLine[wx];
            var s = southLine[x];
            var se = southLine[ex];

            dx = (ne + (2 * e) + se) - (nw + (2 * w) + sw);
            dy = (sw + (2 * s) + se) - (nw + (2 * n) + ne);
        }

    }
}
