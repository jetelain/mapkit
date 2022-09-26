using System.Collections.Generic;
using SixLabors.Fonts;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawText : IDrawOperation
    {
        public DrawText(Vector point, string text, MemDrawTextStyle style)
        {
            Point = point;
            Text = text;
            TextStyle = style;

            var to = new TextOptions(style.Font);
            to.VerticalAlignment = style.VerticalAlignment;
            to.HorizontalAlignment = style.HorizontalAlignment;

            var measure = TextMeasurer.MeasureBounds(Text, to);
            Min = new Vector(point.X + measure.X, point.Y + measure.Y);
            Max = new Vector(point.X + measure.X + measure.Width * 1.25, point.Y + measure.Y + measure.Height * 1.25); // 25% margin due to different raster engines
        }

        public Vector Point { get; }
        public string Text { get; }
        public MemDrawTextStyle TextStyle { get; }
        public Vector Min { get; }
        public Vector Max { get; }

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