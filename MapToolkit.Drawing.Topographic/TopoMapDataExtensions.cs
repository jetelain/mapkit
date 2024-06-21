namespace MapToolkit.Drawing.Topographic
{
    public static class TopoMapDataExtensions
    {
        public static ITopoMapData Crop(this ITopoMapData other, Coordinates min, Coordinates max, string? title = null)
        {
            return new TopoMapData()
            {
                Title = title ?? other.Title,
                DemDataCell = other.DemDataCell.CreateView(min, max),
                ForestPolygons = other.ForestPolygons?.Crop(min, max),
                RockPolygons = other.RockPolygons?.Crop(min, max),
                FortPolygons = other.FortPolygons?.Crop(min, max),
                BuildingPolygons = other.BuildingPolygons?.Crop(min, max),
                WaterPolygons = other.WaterPolygons?.Crop(min, max),
                Bridges = other.Bridges?.ToDictionary(k => k.Key, k => k.Value.Crop(min, max)),
                Roads = other.Roads?.ToDictionary(k => k.Key, k => k.Value.Crop(min, max)),
                Powerlines = other.Powerlines?.Crop(min, max),
                Names = other.Names?.Where(n => n.Position?.IsInSquare(min, max) ?? false)?.ToList(),
                Icons = other.Icons?.Where(n => n.Coordinates?.IsInSquare(min, max) ?? false)?.ToList(),
                PlottedPoints = other.PlottedPoints?.Where(n => n.Coordinates?.IsInSquare(min, max) ?? false)?.ToList()
            };
        }
    }
}
