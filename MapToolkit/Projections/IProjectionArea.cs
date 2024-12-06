using System;

namespace Pmad.Cartography.Projections
{
    public interface IProjectionArea
    {
        Vector Project(CoordinatesValue coordinates);

        Vector[] Project(ReadOnlySpan<CoordinatesValue> coordinates);

        Vector Min { get; }

        Vector Size { get; }
    }
}