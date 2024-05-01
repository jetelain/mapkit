using System;
using System.Linq;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MapToolkit.DataCells
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

        /// <summary>
        /// Legend for Earth (20000m range)
        /// </summary>
        /// <returns></returns>
        public static DemLegend CreateAbsolute()
        {
            return new DemLegend(
                new DemLegendPoint(-11000, Color.ParseHex("000000").ToPixel<Rgb24>()), // Challenger Deep 10,902–10,929 m
                new DemLegendPoint(-5000, Color.ParseHex("000080").ToPixel<Rgb24>()),
                new DemLegendPoint(-1000, Color.ParseHex("0026FF").ToPixel<Rgb24>()),
                new DemLegendPoint(-500, Color.ParseHex("004A7F").ToPixel<Rgb24>()),
                new DemLegendPoint(0, Color.ParseHex("0094FF").ToPixel<Rgb24>()),
                new DemLegendPoint(1, Color.ParseHex("7FC9FF").ToPixel<Rgb24>()),
                new DemLegendPoint(100, Color.ParseHex("AFF0E8").ToPixel<Rgb24>()),
                new DemLegendPoint(200, Color.ParseHex("B1F4BF").ToPixel<Rgb24>()),
                new DemLegendPoint(300, Color.ParseHex("D0FAB2").ToPixel<Rgb24>()),
                new DemLegendPoint(400, Color.ParseHex("F2F9A9").ToPixel<Rgb24>()),
                new DemLegendPoint(500, Color.ParseHex("88CB57").ToPixel<Rgb24>()),
                new DemLegendPoint(600, Color.ParseHex("1D9E2A").ToPixel<Rgb24>()),
                new DemLegendPoint(700, Color.ParseHex("28863C").ToPixel<Rgb24>()),
                new DemLegendPoint(800, Color.ParseHex("7E9B2F").ToPixel<Rgb24>()),
                new DemLegendPoint(900, Color.ParseHex("CBAF1D").ToPixel<Rgb24>()),
                new DemLegendPoint(1000, Color.ParseHex("EA9401").ToPixel<Rgb24>()),
                new DemLegendPoint(1200, Color.ParseHex("BD4602").ToPixel<Rgb24>()),
                new DemLegendPoint(1400, Color.ParseHex("930F02").ToPixel<Rgb24>()),
                new DemLegendPoint(1600, Color.ParseHex("751404").ToPixel<Rgb24>()),
                new DemLegendPoint(1800, Color.ParseHex("702309").ToPixel<Rgb24>()),
                new DemLegendPoint(2000, Color.ParseHex("6D2E0B").ToPixel<Rgb24>()),
                new DemLegendPoint(2500, Color.ParseHex("7D4A2B").ToPixel<Rgb24>()),
                new DemLegendPoint(3000, Color.ParseHex("96715F").ToPixel<Rgb24>()),
                new DemLegendPoint(4000, Color.ParseHex("A8A3A0").ToPixel<Rgb24>()),
                new DemLegendPoint(5000, Color.ParseHex("C2C2C2").ToPixel<Rgb24>()),
                new DemLegendPoint(6000, Color.ParseHex("E2DDE3").ToPixel<Rgb24>()),
                new DemLegendPoint(8000, Color.ParseHex("FFFBFF").ToPixel<Rgb24>()),
                new DemLegendPoint(9000, Color.ParseHex("FFFFFF").ToPixel<Rgb24>()) // Everest  8,848.86 m
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
