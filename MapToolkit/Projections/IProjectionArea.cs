using System;
using GeoJSON.Text.Geometry;
using Pmad.Geometry;

namespace MapToolkit.Projections
{
    public interface IProjectionArea
    {
        Vector Project(Vector2D coordinates);

        Vector Project(CoordinatesS coordinates);

        Vector[] Project(ReadOnlySpan<CoordinatesS> coordinates);

        Vector Min { get; }

        Vector Size { get; }
    }
}