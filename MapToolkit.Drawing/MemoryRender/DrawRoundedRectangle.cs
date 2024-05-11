


namespace MapToolkit.Drawing.MemoryRender
{
    internal class DrawRoundedRectangle : IDrawOperation
    {
        public DrawRoundedRectangle(Vector topLeft, Vector bottomRight, MemDrawStyle style, float radius)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
            Style = style;
            Radius = radius;
        }

        public Vector TopLeft { get; }

        public Vector BottomRight { get; }

        public MemDrawStyle Style { get; }

        public float Radius { get; }

        public Vector Min => TopLeft;

        public Vector Max => BottomRight;

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