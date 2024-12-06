using Pmad.Geometry;

namespace MapToolkit.Drawing.Topographic
{
    public static class TopoMapDataExtensions
    {
        public static ITopoMapData Crop(this ITopoMapData other, CoordinatesValue min, CoordinatesValue max, TopoMapMetadata? metadata = null)
        {
            var range = new VectorEnvelope<Vector2D>(min.Vector2D, max.Vector2D);
            return new TopoMapData()
            {
                Metadata = metadata ?? other.Metadata,
                DemDataCell = other.DemDataCell.CreateView(min, max),
                ForestPolygons = other.ForestPolygons?.Crop(range),
                RockPolygons = other.RockPolygons?.Crop(range),
                FortPolygons = other.FortPolygons?.Crop(range),
                BuildingPolygons = other.BuildingPolygons?.Crop(range),
                WaterPolygons = other.WaterPolygons?.Crop(range),
                Bridges = other.Bridges?.ToDictionary(k => k.Key, k => k.Value.Crop(range)),
                Roads = other.Roads?.ToDictionary(k => k.Key, k => k.Value.Crop(range)),
                Powerlines = other.Powerlines?.Crop(range),
                Railways = other.Railways?.Crop(range),
                Names = other.Names?.Where(n => n.Position.IsInSquare(range))?.ToList(),
                Icons = other.Icons?.Where(n => n.Coordinates.IsInSquare(range))?.ToList(),
                PlottedPoints = other.PlottedPoints?.Where(n => n.CoordinatesS.IsInSquare(range))?.ToList()
            };
        }
    }
}
