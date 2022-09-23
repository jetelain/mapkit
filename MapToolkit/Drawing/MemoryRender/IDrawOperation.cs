using System.Collections.Generic;

namespace MapToolkit.Drawing.MemoryRender
{
    internal interface IDrawOperation
    {
        void DrawClipped(MemDrawClipped context);

        void Draw(MemDrawContext context);

        Vector Min { get; }

        Vector Max { get; }

        IDrawOperation Scale(MemDrawScale context);

        IEnumerable<IDrawOperation> Simplify();
    }
}