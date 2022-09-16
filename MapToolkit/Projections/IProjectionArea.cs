using GeoJSON.Text.Geometry;

namespace SimpleDEM.Projections
{
    public interface IProjectionArea
    {
        Vector Project(IPosition coordinates);

        Vector Min { get; }

        Vector Size { get; }
    }
}