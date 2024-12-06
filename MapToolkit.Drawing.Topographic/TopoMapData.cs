using GeoJSON.Text.Geometry;
using Pmad.Cartography.DataCells;
using Pmad.Geometry.Shapes;
using Pmad.Geometry;

namespace Pmad.Cartography.Drawing.Topographic
{
    internal class TopoMapData : ITopoMapData
    {
        public TopoMapMetadata Metadata { get; set; } = TopoMapMetadata.None;

        public Dictionary<TopoMapPathType, MultiPath<double, Vector2D>>? Roads { get; set; }

        public Dictionary<TopoMapPathType, MultiPath<double, Vector2D>>? Bridges { get; set; }

        public MultiPolygon<double, Vector2D>? ForestPolygons { get; set; }

        public MultiPolygon<double, Vector2D>? RockPolygons { get; set; }

        public MultiPolygon<double, Vector2D>? BuildingPolygons { get; set; }

        public MultiPolygon<double, Vector2D>? WaterPolygons { get; set; }

        public required IDemDataView DemDataCell { get; set; }

        public List<TopoLocation>? Names { get; set; }

        public List<DemDataPoint>? PlottedPoints { get; set; }

        public List<TopoIcon>? Icons { get; set; }

        public MultiPath<double, Vector2D>? Powerlines { get; set; }

        public MultiPath<double, Vector2D>? Railways { get; set; }

        public MultiPolygon<double, Vector2D>? FortPolygons { get; internal set; }
    }
}
