using GeoJSON.Text.Geometry;
using MapToolkit.DataCells;

namespace MapToolkit.Drawing.Topographic
{
    internal class TopoMapData : ITopoMapData
    {
        public string Title { get; set; } = "(no name)";

        public Dictionary<TopoMapPathType, MultiLineString>? Roads { get; set; }

        public Dictionary<TopoMapPathType, MultiLineString>? Bridges { get; set; }

        public MultiPolygon? ForestPolygons { get; set; }

        public MultiPolygon? RockPolygons { get; set; }

        public MultiPolygon? BuildingPolygons { get; set; }

        public MultiPolygon? WaterPolygons { get; set; }

        public required IDemDataView DemDataCell { get; set; }

        public List<TopoLocation>? Names { get; set; }

        public List<DemDataPoint>? PlottedPoints { get; set; }

        public List<TopoIcon>? Icons { get; set; }

        public MultiLineString? Powerlines { get; set; }

        public MultiPolygon? FortPolygons { get; internal set; }
    }
}
