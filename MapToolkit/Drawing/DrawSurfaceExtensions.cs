using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing
{
    public static class DrawSurfaceExtensions
    {
        public static IDrawStyle AllocateBrushStyle(this IDrawSurface surface, Color color)
            => surface.AllocateStyle(new SolidColorBrush(color), null);

        public static IDrawStyle AllocateBrushStyle(this IDrawSurface surface, string hexColor)
            => surface.AllocateStyle(new SolidColorBrush(Color.ParseHex(hexColor)), null);

        public static IDrawStyle AllocatePenStyle(this IDrawSurface surface, Color color, double width = 1, IEnumerable<double>? pattern = null)
            => surface.AllocateStyle(null, new Pen(color, width, pattern));

        public static IDrawStyle AllocatePenStyle(this IDrawSurface surface, string hexColor, double width = 1, IEnumerable<double>? pattern = null)
            => surface.AllocateStyle(null, new Pen(Color.ParseHex(hexColor), width, pattern));

        public static void DrawRectangle(this IDrawSurface surface, Vector topLeft, Vector bottomRight, IDrawStyle style)
        {
            surface.DrawPolygon(new[] {
                    topLeft,
                    new Vector(bottomRight.X, topLeft.Y),
                    bottomRight,
                    new Vector(topLeft.X, bottomRight.Y),
                    topLeft
                }, style);
        }


    }
}
