namespace Pmad.Cartography.Drawing
{
    public class TilingInfos
    {
        public int MaxZoom { get; internal set; }
        public int MinZoom { get; internal set; }
        public Vector TileSize { get; internal set; } = Vector.Zero;
        public string TilePattern { get; internal set; } = string.Empty;
    }
}