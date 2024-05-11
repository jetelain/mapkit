using System;
using System.Collections.Generic;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace MapToolkit.Drawing
{
    public interface IDrawSurface
    {
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

        IDrawIcon AllocateIcon(Vector size, Action<IDrawSurface> draw);

        void DrawPolygon(IEnumerable<Vector> contour, IDrawStyle style);

        void DrawPolygon(IEnumerable<Vector> contour, IEnumerable<IEnumerable<Vector>> holes, IDrawStyle style);

        void DrawPolyline(IEnumerable<Vector> points, IDrawStyle style);

        void DrawCircle(Vector center, float radius, IDrawStyle style);

        void DrawArc(Vector center, float radius, float startAngle, float sweepAngle, IDrawStyle style);

        void DrawImage(Image image, Vector pos, Vector size, double alpha);

        void DrawTextPath(IEnumerable<Vector> points, string text, IDrawTextStyle style);

        void DrawText(Vector point, string text, IDrawTextStyle style);

        void DrawIcon(Vector center, IDrawIcon icon);

        void DrawRoundedRectangle(Vector topLeft, Vector bottomRight, IDrawStyle style, float radius);
    }
}
