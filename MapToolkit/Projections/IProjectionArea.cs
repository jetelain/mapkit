using System;
using Pmad.Geometry;

namespace Pmad.Cartography.Projections
{
    public interface IProjectionArea
    {
        Vector2D Project(CoordinatesValue coordinates);

        Vector2D[] Project(ReadOnlySpan<CoordinatesValue> coordinates);

        Vector2D Min { get; }

        Vector2D Size { get; }
    }
}