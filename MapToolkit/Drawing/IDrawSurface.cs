using System.Collections.Generic;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing
{
    public interface IDrawSurface
    {
        double Scale { get; }

        IDrawStyle AllocateStyle(
            IBrush? fill,
            Pen? pen);

        IDrawTextStyle AllocateTextStyle(
            string[] fontNames,
            FontStyle style,
            double size,
            IBrush? fill,
            Pen? pen,
            bool fillCoverPen = false,
            TextAnchor textAnchor = TextAnchor.CenterLeft);

        void DrawPolygon(IEnumerable<Vector> contour, IDrawStyle style);

        void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style);

        void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style);

        void DrawCircle(Vector center, float radius, IDrawStyle style);

        void DrawImage(Image image, Vector pos, Vector size, double alpha);

        void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style);

        void DrawText(Vector point, string text, IDrawTextStyle style);
    }
}
