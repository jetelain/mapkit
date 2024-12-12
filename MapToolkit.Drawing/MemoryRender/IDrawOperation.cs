using System.Collections.Generic;
using Pmad.Geometry;

namespace Pmad.Drawing.MemoryRender
{
    internal interface IDrawOperation
    {
        void DrawClipped(MemDrawClipped context);

        void Draw(MemDrawContext context);

        Vector2D Min { get; }

        Vector2D Max { get; }

        IDrawOperation Scale(MemDrawScale context);

        IEnumerable<IDrawOperation> Simplify(double lengthSquared);
    }
}