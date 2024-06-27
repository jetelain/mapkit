using GeoJSON.Text.Geometry;
using MapToolkit.DataCells;

namespace MapToolkit.Drawing.Topographic
{
    public interface ITopoMapData
    {
        TopoMapMetadata Metadata { get; }

        Dictionary<TopoMapPathType, MultiLineString>? Roads { get; }

        Dictionary<TopoMapPathType, MultiLineString>? Bridges { get; }

        MultiPolygon? ForestPolygons { get; }

        MultiPolygon? RockPolygons { get; }

        MultiPolygon? BuildingPolygons { get; }

        MultiPolygon? WaterPolygons { get; }

        IDemDataView DemDataCell { get; }

        List<TopoLocation>? Names { get; }

        List<TopoIcon>? Icons { get; }

        MultiLineString? Powerlines { get; }

        MultiPolygon? FortPolygons { get; }

        List<DemDataPoint>? PlottedPoints { get; }
    }
}
