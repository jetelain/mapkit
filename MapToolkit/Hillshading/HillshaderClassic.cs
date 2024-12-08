using System;

namespace Pmad.Cartography.Hillshading
{
    /// <summary>
    /// Classic hillshader.
    /// </summary>
    public sealed class HillshaderClassic : HillshaderBase
    {
        private readonly GradientBase gradient;
        private readonly double zenithCos;
        private readonly double zenithSin;
        private readonly double azimuthRad;

        public HillshaderClassic(Vector? resolution = null, double elevation = 35, double azimuth = 315, double factor = 1)
        {
            var zenithRad = (90.0 - elevation) * Math.PI / 180.0;
            zenithCos = Math.Cos(zenithRad);
            zenithSin = Math.Sin(zenithRad);

            var azimuthMath = 360.0 - azimuth + 90.0;
            if (azimuthMath >= 360.0)
            {
                azimuthMath = azimuthMath - 360.0;
            }
            azimuthRad = azimuthMath * Math.PI / 180.0;

            this.gradient = new Horn(resolution ?? Vector.One, factor);
        }

        protected override double Flat => zenithCos;

        protected override double GetPixelLuminance(double[] southLine, double[] line, double[] northLine, int x)
        {
            gradient.GetSlopeAndAspect(southLine, line, northLine, x, out var slope, out var aspect);
            var lum = (zenithCos * Math.Cos(slope)) + (zenithSin * Math.Sin(slope) * Math.Cos(azimuthRad - aspect));
            if (lum < 0)
            {
                lum = 0;
            }
            else if (lum > 1)
            {
                lum = 1;
            }
            return lum;
        }
    }
}
