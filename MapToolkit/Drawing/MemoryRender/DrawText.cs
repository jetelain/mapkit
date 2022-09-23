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
            Style = style;

            var to = new TextOptions(style.Font);
            to.VerticalAlignment = style.VerticalAlignment;
            to.HorizontalAlignment = style.HorizontalAlignment;

            var measure = TextMeasurer.Measure(Text, to);
            Min = new Vector(point.X - 2, point.Y - 2);
            Max = new Vector(point.X + measure.Width + 2, point.Y + measure.Height + 2);
        }

        public Vector Point { get; }
        public string Text { get; }
        public MemDrawTextStyle Style { get; }
        public Vector Min { get; }
        public Vector Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawText(Point, Text, context.MapTextStyle(Style));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawText(context.Translate(Point), Text, context.MapTextStyle(Style));
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawText(Point * context.Scale, Text, context.MapTextStyle(Style));
        }

        public IEnumerable<IDrawOperation> Simplify()
        {
            yield return this;
        }
    }
}