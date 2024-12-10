using System.Collections.Generic;
using Pmad.Geometry;

namespace Pmad.Cartography.Drawing.MemoryRender
{
    internal class DrawCircle : IDrawOperation
    {
        public DrawCircle(Vector2D center, float radius, MemDrawStyle style)
        {
            Center = center;
            Radius = radius;
            Style = style;

            Min = Center - new Vector2D(radius, radius);
            Max = Center + new Vector2D(radius, radius);
        }

        public Vector2D Center { get; }

        public float Radius { get; }

        public MemDrawStyle Style { get; }
        public Vector2D Min { get; }
        public Vector2D Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawCircle(Center, Radius, context.MapStyle(Style));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawCircle(context.Translate(Center), Radius, context.MapStyle(Style));
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawCircle(Center * context.Scale, (float)(Radius * context.Scale), context.MapStyle(Style));
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