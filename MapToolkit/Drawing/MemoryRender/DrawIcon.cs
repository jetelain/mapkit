using System.Collections.Generic;

namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawIcon : IDrawOperation
    {
        public DrawIcon(Vector center, MemDrawIcon icon)
        {
            Center = center;
            Icon = icon;
            Min = Center - (icon.Size / 2);
            Max = Center + (icon.Size / 2);
        }

        public Vector Center { get; }

        public MemDrawIcon Icon { get; }

        public Vector Min { get; }

        public Vector Max { get; }

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawIcon(Center, context.MapIcon(Icon));
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawIcon(context.Translate(Center), context.MapIcon(Icon));
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawIcon(Center * context.Scale, context.MapIcon(Icon));
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared = 9)
        {
            yield return this;
        }
    }
}