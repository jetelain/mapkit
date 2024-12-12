using System;
using System.Collections.Generic;
using System.Linq;
using Pmad.Geometry;

namespace Pmad.Drawing.MemoryRender
{
    internal class MemDrawIcon : IDrawIcon
    {
        public MemDrawIcon(Vector2D size, List<IDrawOperation> drawOperations)
        {
            Size = size;
            DrawOperations = drawOperations;
        }

        public Vector2D Size { get; }

        public List<IDrawOperation> DrawOperations { get; }

        internal void Draw(IDrawSurface target, MemDrawContext context)
        {
            var subContext = new MemDrawContext(context, target);
            foreach(var operation in DrawOperations)
            {
                operation.Draw(subContext);
            }
        }

        internal MemDrawIcon Scale(MemDrawScale context)
        {
            return new MemDrawIcon(Size * context.Scale, DrawOperations.Select(o => o.Scale(context)).ToList());
        }
    }
}