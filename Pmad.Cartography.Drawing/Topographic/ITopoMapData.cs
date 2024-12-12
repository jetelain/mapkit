using GeoJSON.Text.Geometry;
using Pmad.Cartography.DataCells;
using Pmad.Geometry.Shapes;
using Pmad.Geometry;

namespace Pmad.Cartography.Drawing.Topographic
{
    public interface ITopoMapData
    {
        TopoMapMetadata Metadata { get; }

        Dictionary<TopoMapPathType, MultiPath<double, Vector2D>>? Roads { get; }

        Dictionary<TopoMapPathType, MultiPath<double, Vector2D>>? Bridges { get; }

        MultiPolygon<double, Vector2D>? ForestPolygons { get; }

        MultiPolygon<double, Vector2D>? RockPolygons { get; }

        MultiPolygon<double, Vector2D>? BuildingPolygons { get; }

        MultiPolygon<double, Vector2D>? WaterPolygons { get; }

        IDemDataView DemDataCell { get; }

        List<TopoLocation>? Names { get; }

        List<TopoIcon>? Icons { get; }

        MultiPath<double, Vector2D>? Powerlines { get; }

        MultiPath<double, Vector2D>? Railways { get; }

        MultiPolygon<double, Vector2D>? FortPolygons { get; }

        List<DemDataPoint>? PlottedPoints { get; }
    }
}
