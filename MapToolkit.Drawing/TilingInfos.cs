using Pmad.Geometry;

namespace Pmad.Cartography.Drawing
{
    public class TilingInfos
    {
        public int MaxZoom { get; internal set; }
        public int MinZoom { get; internal set; }
        public Vector2D TileSize { get; internal set; } = Vector2D.Zero;
        public string TilePattern { get; internal set; } = string.Empty;
    }
}