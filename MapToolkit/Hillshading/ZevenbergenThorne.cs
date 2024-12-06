using System;
using Pmad.Geometry;

namespace Pmad.Cartography.Hillshading
{
    public sealed class ZevenbergenThorne : GradientBase
    {

        public ZevenbergenThorne(Vector2D resolution, double factor = 1)
            : base(resolution, factor, 2)
        {

        }

        internal override void GetDeltaUnscaled(double[] southLine, double[] line, double[] northLine, int x, out double dx, out double dy)
        {
            var wx = Math.Max(0, x - 1);
            var ex = Math.Min(line.Length - 1, x + 1);

            var n = northLine[x];
            var w = line[wx];
            var e = line[ex];
            var s = southLine[x];

            dx = e - w;
            dy = s - n;
        }

    }
}
