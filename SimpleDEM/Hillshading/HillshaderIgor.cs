using System;

namespace SimpleDEM.Hillshading
{
    public sealed class HillshaderIgor : HillshaderBase
    {
        private readonly GradientBase gradient;
        private readonly double azimuthRad;

        public HillshaderIgor(double azimuth = 315, double factor = 0.5)
        {
            gradient = new Horn(factor);
            azimuthRad = azimuth * Math.PI / 180.0;
        }

        protected override double GetPixelLuminance(double[] southLine, double[] line, double[] northLine, int x)
        {
            gradient.GetSlopeAndAspect(southLine, line, northLine, x, out var slope, out var aspect);

            var slopeStrength = slope / (Math.PI / 2);
            var aspectDiff = DifferenceBetweenAngles(aspect, Math.PI * 3 / 2 - azimuthRad, Math.PI * 2);
            var aspectStrength = 1 - aspectDiff / Math.PI;

            return 1.0 - slopeStrength * aspectStrength;
        }

        protected override double Flat => 1;

        static double NormalizeAngle(double angle, double normalizer)
        {
            angle = angle % normalizer;
            if (angle < 0)
            {
                angle = normalizer + angle;
            }

            return angle;
        }

        static double DifferenceBetweenAngles(double angle1, double angle2, double normalizer)
        {
            var diff = Math.Abs(NormalizeAngle(angle1, normalizer) - NormalizeAngle(angle2, normalizer));
            if (diff > normalizer / 2)
            {
                diff = normalizer - diff;
            }
            return diff;
        }
    }
}
