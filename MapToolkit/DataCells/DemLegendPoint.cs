using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.Cartography.DataCells
{
    internal class DemLegendPoint
    {
        public DemLegendPoint(double elevation, Rgb24 color)
        {
            E = elevation;
            Color = color.ToScaledVector4();
        }

        public double E { get; }

        public Vector4 Color { get; }
    }
}