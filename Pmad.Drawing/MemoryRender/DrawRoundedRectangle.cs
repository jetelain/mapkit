


using Pmad.Geometry;

namespace Pmad.Drawing.MemoryRender
{
    internal class DrawRoundedRectangle : IDrawOperation
    {
        public DrawRoundedRectangle(Vector2D topLeft, Vector2D bottomRight, MemDrawStyle style, float radius)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
            Style = style;
            Radius = radius;
        }

        public Vector2D TopLeft { get; }

        public Vector2D BottomRight { get; }

        public MemDrawStyle Style { get; }

        public float Radius { get; }

        public Vector2D Min => TopLeft;

        public Vector2D Max => BottomRight;

        public void Draw(MemDrawContext context)
        {
            context.Target.DrawRoundedRectangle(TopLeft, BottomRight, context.MapStyle(Style), Radius);
        }

        public void DrawClipped(MemDrawClipped context)
        {
            context.Target.DrawRoundedRectangle(TopLeft, BottomRight, context.MapStyle(Style), Radius);
        }

        public IDrawOperation Scale(MemDrawScale context)
        {
            return new DrawRoundedRectangle(TopLeft * context.Scale, BottomRight * context.Scale, context.MapStyle(Style), (float)(Radius * context.Scale));
        }

        public IEnumerable<IDrawOperation> Simplify(double lengthSquared)
        {
            yield return this;
        }
    }
}