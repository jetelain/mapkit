using Pmad.Geometry;
using SixLabors.ImageSharp;

namespace Pmad.Drawing
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

        public static IDrawStyle AllocateStyle(this IDrawSurface surface, Color fillColor, Color penColor, double width = 1, IEnumerable<double>? pattern = null)
            => surface.AllocateStyle(new SolidColorBrush(fillColor), new Pen(penColor, width, pattern));

        public static IDrawStyle AllocateStyle(this IDrawSurface surface, string fillHexColor, string penHexColor, double width = 1, IEnumerable<double>? pattern = null)
            => surface.AllocateStyle(new SolidColorBrush(Color.ParseHex(fillHexColor)), new Pen(Color.ParseHex(penHexColor), width, pattern));

        public static void DrawRectangle(this IDrawSurface surface, Vector2D topLeft, Vector2D bottomRight, IDrawStyle style)
        {
            surface.DrawPolygon([new Vector2D[] {
                    topLeft,
                    new Vector2D(bottomRight.X, topLeft.Y),
                    bottomRight,
                    new Vector2D(topLeft.X, bottomRight.Y),
                    topLeft
                }], style);
        }

        public static void DrawPolygon(this IDrawSurface surface, Vector2D[] contour, IDrawStyle style)
        {
            surface.DrawPolygon([contour], style);
        }

        public static void DrawPolygon(this IDrawSurface surface, Vector2D[] contour, IEnumerable<Vector2D[]> holes, IDrawStyle style)
        {
            var paths = new List<Vector2D[]>();
            paths.Add(contour);
            paths.AddRange(holes);
            surface.DrawPolygon(paths, style);
        }


    }
}
