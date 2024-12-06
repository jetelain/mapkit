using System;

namespace MapToolkit.Projections
{
    public interface IProjectionArea
    {
        Vector Project(CoordinatesValue coordinates);

        Vector[] Project(ReadOnlySpan<CoordinatesValue> coordinates);

        Vector Min { get; }

        Vector Size { get; }
    }
}