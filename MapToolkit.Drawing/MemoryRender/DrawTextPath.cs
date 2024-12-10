using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pmad.Geometry;
using SixLabors.Fonts;

namespace Pmad.Cartography.Drawing.MemoryRender
{
    internal class DrawTextPath : IDrawOperation
    {
        public DrawTextPath(List<Vector2D> points, string text, MemDrawTextStyle style)
        {
            Points = points;
            Text = text;
            TextStyle = style;

            var first = points.First();
            var last = points.Last();
            var angle = Math.Atan2(last.Y - first.Y, last.X - first.X);
            var matrix = Matrix3x2.CreateRotation((float)angle, new Vector2((float)first.X, (float)first.Y));

            var to = new TextOptions(style.Font);
            to.VerticalAlignment = style.VerticalAlignment;
            to.HorizontalAlignment = style.HorizontalAlignment;

            var measure = TextMeasurer.MeasureSize(Text, to);

            var measurePoints = new[] {
                new Vector2((float)first.X, (float)first.Y),
                new Vector2((float)first.X+measure.Height, (float)first.Y),
                new Vector2((float)first.X+measure.Height, (float)first.Y+measure.Width),
                new Vector2((float)first.X, (float)first.Y+measure.Width)
                }.Select(p => Vector2.Transform(p, matrix)).ToList();

            Min = new Vector2D(measurePoints.Min(v => v.X - 2), measurePoints.Min(v => v.Y - 2));
            Max = new Vector2D(measurePoints.Max(v => v.X + 2), measurePoints.Max(v => v.Y + 2));
        }

        public List<Vector2D> Points { get; }

        public string Text { get; }

        public MemDrawTextStyle TextStyle { get; }

        public Vector2D Min { get; }

        public Vector2D Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawTextPath(Points, Text, context.MapTextStyle(TextStyle));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawTextPath(Points.Select(context.Translate), Text, context.MapTextStyle(TextStyle));
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawTextPath(Points.Select(p => p * context.Scale).ToList(), Text, context.MapTextStyle(TextStyle));
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared = 9)
        {
            var path = LevelOfDetailHelper.SimplifyAnglesAndDistances(Points, lengthSquared);
            if (path.Count > 0)
            {
                yield return new DrawTextPath(path, Text, TextStyle);
            }
        }
    }
}