using System;
using System.Linq;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pmad.Cartography.DataCells
{
    internal class DemLegend
    {
        private readonly DemLegendPoint[] points;

        public DemLegend(params DemLegendPoint[] points)
        {
            this.points = points;
        }

        public Rgb24 ToRgb24(double elevation)
        {
            var before = points.Where(e => e.E <= elevation).Last();
            var after = points.FirstOrDefault(e => e.E > elevation) ?? points.Last();
            var scale = (float)((elevation - before.E) / (after.E - before.E));
            Rgb24 rgb = new Rgb24();
            rgb.FromScaledVector4(Vector4.Lerp(before.Color, after.Color, scale));
            return rgb;
        }

        public static DemLegend CreateRelative(double min, double max)
        {
            return new DemLegend(
                new DemLegendPoint(min, Color.LightBlue.ToPixel<Rgb24>()),
                new DemLegendPoint(min + (max - min) * 0.10, Color.DarkGreen.ToPixel<Rgb24>()),
                new DemLegendPoint(min + (max - min) * 0.15, Color.Green.ToPixel<Rgb24>()),
                new DemLegendPoint(min + (max - min) * 0.40, Color.Yellow.ToPixel<Rgb24>()),
                new DemLegendPoint(min + (max - min) * 0.70, Color.Red.ToPixel<Rgb24>()),
                new DemLegendPoint(max, Color.Maroon.ToPixel<Rgb24>())
            );
        }

        public static DemLegend CreateRelative(IDemDataView view)
        {
            var min = double.MaxValue;
            var max = double.MinValue;
            for (int lat = 0; lat < view.PointsLat; lat++)
            {
                foreach (var point in view.GetPointsOnParallel(lat, 0, view.PointsLon))
                {
                    if (!double.IsNaN(point.Elevation))
                    {
                        max = Math.Max(point.Elevation, max);
                        min = Math.Min(point.Elevation, min);
                    }
                }
            }
            return CreateRelative(min, max);
        }
    }
}
