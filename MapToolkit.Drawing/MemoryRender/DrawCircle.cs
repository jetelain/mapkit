using System.Collections.Generic;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawCircle : IDrawOperation
    {
        public DrawCircle(Vector center, float radius, MemDrawStyle style)
        {
            Center = center;
            Radius = radius;
            Style = style;

            Min = Center - new Vector(radius, radius);
            Max = Center + new Vector(radius, radius);
        }

        public Vector Center { get; }

        public float Radius { get; }

        public MemDrawStyle Style { get; }
        public Vector Min { get; }
        public Vector Max { get; }

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