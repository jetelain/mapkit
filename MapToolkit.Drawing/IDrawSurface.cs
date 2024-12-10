using System;
using System.Collections.Generic;
using Pmad.Geometry;
using Pmad.Geometry.Collections;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Pmad.Cartography.Drawing
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

        IDrawIcon AllocateIcon(Vector2D size, Action<IDrawSurface> draw);

        void DrawPolygon(IEnumerable<Vector2D[]> paths, IDrawStyle style);

        void DrawPolyline(IEnumerable<Vector2D> points, IDrawStyle style);

        void DrawCircle(Vector2D center, float radius, IDrawStyle style);

        void DrawArc(Vector2D center, float radius, float startAngle, float sweepAngle, IDrawStyle style);

        void DrawImage(Image image, Vector2D pos, Vector2D size, double alpha);

        void DrawTextPath(IEnumerable<Vector2D> points, string text, IDrawTextStyle style);

        void DrawText(Vector2D point, string text, IDrawTextStyle style);

        void DrawIcon(Vector2D center, IDrawIcon icon);

        void DrawRoundedRectangle(Vector2D topLeft, Vector2D bottomRight, IDrawStyle style, float radius);
    }
}
