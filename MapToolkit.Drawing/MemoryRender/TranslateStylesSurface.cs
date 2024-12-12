using System;
using System.Collections.Generic;
using Pmad.Geometry;
using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Pmad.Drawing.MemoryRender
{
    internal class TranslateStylesSurface : IDrawSurface
    {
        private readonly IRemapStyle memDrawContext;
        private readonly IDrawSurface s;

        public TranslateStylesSurface(IRemapStyle memDrawContext, IDrawSurface s)
        {
            this.memDrawContext = memDrawContext;
            this.s = s;
        }

        public IDrawIcon AllocateIcon(Vector2D size, Action<IDrawSurface> draw)
        {
            throw new NotImplementedException();
        }

        public IDrawStyle AllocateStyle(IBrush? fill, Pen? pen)
        {
            throw new System.NotImplementedException();
        }

        public IDrawTextStyle AllocateTextStyle(string[] fontNames, FontStyle style, double size, IBrush? fill, Pen? pen, bool fillCoverPen = false, TextAnchor textAnchor = TextAnchor.CenterLeft)
        {
            throw new System.NotImplementedException();
        }

        public void DrawArc(Vector2D center, float radius, float startAngle, float sweepAngle, IDrawStyle style)
        {
            s.DrawArc(center, radius, startAngle, sweepAngle, memDrawContext.MapStyle((MemDrawStyle)style));
        }

        public void DrawCircle(Vector2D center, float radius, IDrawStyle style)
        {
            s.DrawCircle(center, radius, memDrawContext.MapStyle((MemDrawStyle)style));
        }

        public void DrawIcon(Vector2D center, IDrawIcon icon)
        {
            throw new NotImplementedException();
        }

        public void DrawImage(Image image, Vector2D pos, Vector2D size, double alpha)
        {
            throw new System.NotImplementedException();
        }

        public void DrawPolygon(IEnumerable<Vector2D[]> paths, IDrawStyle style)
        {
            throw new System.NotImplementedException();
        }

        public void DrawPolyline(IEnumerable<Vector2D> points, IDrawStyle style)
        {
            throw new System.NotImplementedException();
        }

        public void DrawRoundedRectangle(Vector2D topLeft, Vector2D bottomRight, IDrawStyle style, float radius)
        {
            s.DrawRoundedRectangle(topLeft, bottomRight, memDrawContext.MapStyle((MemDrawStyle)style), radius);
        }

        public void DrawText(Vector2D point, string text, IDrawTextStyle style)
        {
            throw new System.NotImplementedException();
        }

        public void DrawTextPath(IEnumerable<Vector2D> points, string text, IDrawTextStyle style)
        {
            throw new System.NotImplementedException();
        }
    }
}