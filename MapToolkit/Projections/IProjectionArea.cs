using GeoJSON.Text.Geometry;

namespace MapToolkit.Projections
{
    public interface IProjectionArea
    {
        Vector Project(IPosition coordinates);

        Vector Min { get; }

        Vector Size { get; }
    }
}