using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SixLabors.Fonts;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawTextPath : IDrawOperation
    {
        public DrawTextPath(List<Vector> points, string text, MemDrawTextStyle style)
        {
            Points = points;
            Text = text;
            Style = style;

            var first = points.First();
            var last = points.Last();
            var angle = Math.Atan2(last.Y - first.Y, last.X - first.X);
            var matrix = Matrix3x2.CreateRotation((float)angle, new Vector2((float)first.X, (float)first.Y));

            var to = new TextOptions(style.Font);
            to.VerticalAlignment = style.VerticalAlignment;
            to.HorizontalAlignment = style.HorizontalAlignment;

            var measure = TextMeasurer.Measure(Text, to);

            var measurePoints = new[] {
                new Vector2((float)first.X, (float)first.Y),
                new Vector2((float)first.X+measure.Height, (float)first.Y),
                new Vector2((float)first.X+measure.Height, (float)first.Y+measure.Width),
                new Vector2((float)first.X, (float)first.Y+measure.Width)
                }.Select(p => Vector2.Transform(p, matrix)).ToList();

            Min = new Vector(measurePoints.Min(v => v.X - 2), measurePoints.Min(v => v.Y - 2));
            Max = new Vector(measurePoints.Max(v => v.X + 2), measurePoints.Max(v => v.Y + 2));
        }

        public List<Vector> Points { get; }

        public string Text { get; }

        public MemDrawTextStyle Style { get; }

        public Vector Min { get; }

        public Vector Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawTextPath(Points, Text, context.MapTextStyle(Style));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawTextPath(Points.Select(context.Translate), Text, context.MapTextStyle(Style));
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawTextPath(Points.Select(p => p * context.Scale).ToList(), Text, context.MapTextStyle(Style));
        }

        public IEnumerable<IDrawOperation> Simplify()
        {
            var path = DrawHelper.SimplifyOpen(Points);
            if (path != null)
            {
                yield return new DrawTextPath(path, Text, Style);
            }
        }
    }
}