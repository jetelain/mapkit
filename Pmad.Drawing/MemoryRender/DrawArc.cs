using System.Collections.Generic;
using Pmad.Geometry;

namespace Pmad.Drawing.MemoryRender
{
    internal class DrawArc : IDrawOperation
    {
        public DrawArc(Vector2D center, float radius, float startAngle, float sweepAngle, MemDrawStyle style)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
            Style = style;
            Min = Center - new Vector2D(radius, radius);
            Max = Center + new Vector2D(radius, radius);
        }

        public Vector2D Center { get; }

        public float Radius { get; }

        public float StartAngle { get; }

        public float SweepAngle { get; }

        public MemDrawStyle Style { get; }

        public Vector2D Min { get; }

        public Vector2D Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawArc(Center, Radius, StartAngle, SweepAngle, context.MapStyle(Style));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawArc(context.Translate(Center), Radius, StartAngle, SweepAngle, context.MapStyle(Style));
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawArc(Center * context.Scale, (float)(Radius * context.Scale), StartAngle, SweepAngle, context.MapStyle(Style));
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared = 9)
        {
            if (Radius >= 1)
            {
                yield return this;
            }
        }
    }
}