using System.Collections.Generic;
using Pmad.Geometry;
using SixLabors.Fonts;

namespace Pmad.Drawing.MemoryRender
{
    internal class DrawText : IDrawOperation
    {
        public DrawText(Vector2D point, string text, MemDrawTextStyle style)
        {
            Point = point;
            Text = text;
            TextStyle = style;

            var to = new TextOptions(style.Font);
            to.VerticalAlignment = style.VerticalAlignment;
            to.HorizontalAlignment = style.HorizontalAlignment;

            var measure = TextMeasurer.MeasureBounds(Text, to);
            Min = new Vector2D(point.X + measure.X, point.Y + measure.Y);
            Max = new Vector2D(point.X + measure.X + measure.Width * 1.25, point.Y + measure.Y + measure.Height * 1.25); // 25% margin due to different raster engines
        }

        public Vector2D Point { get; }
        public string Text { get; }
        public MemDrawTextStyle TextStyle { get; }
        public Vector2D Min { get; }
        public Vector2D Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawText(Point, Text, context.MapTextStyle(TextStyle));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawText(context.Translate(Point), Text, context.MapTextStyle(TextStyle));
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawText(Point * context.Scale, Text, context.MapTextStyle(TextStyle));
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared = 9)
        {
            yield return this;
        }
    }
}