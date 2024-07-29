using System;

namespace MapToolkit.Projections
{
    public interface IProjectionArea
    {
        Vector Project(CoordinatesS coordinates);

        Vector[] Project(ReadOnlySpan<CoordinatesS> coordinates);

        Vector Min { get; }

        Vector Size { get; }
    }
}