using System;

namespace SimpleDEM.Hillshading
{
    /// <summary>
    /// Almost same result of <see cref="HillshaderClassic"/> but twice as fast
    /// </summary>
    public sealed class HillshaderFast : HillshaderBase
    {
        private readonly GradientBase gradient;

        private readonly double sinAlt;
        private readonly double cosAltSinAz;
        private readonly double cosAltCosAz;

        public HillshaderFast(double elevation = 35, double azimuth = 225, double factor = 0.1)
        {
            var azimuthRad = Math.PI / 180 * azimuth;
            var elevationRad = Math.PI / 180 * elevation;
            gradient = new ZevenbergenThorne(factor);
            sinAlt = Math.Sin(elevationRad);
            cosAltSinAz = Math.Cos(elevationRad) * Math.Sin(azimuthRad);
            cosAltCosAz = Math.Cos(elevationRad) * Math.Cos(azimuthRad);
        }

        protected override double Flat => sinAlt;

        protected override double GetPixelLuminance(double[] southLine, double[] line, double[] northLine, int x)
        {
            gradient.GetDelta(southLine, line, northLine, x, out var dx, out var dy);
            var lum = (sinAlt - cosAltSinAz * dx - cosAltCosAz * dy) / Math.Sqrt(1 + dx * dx + dy * dy);
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
