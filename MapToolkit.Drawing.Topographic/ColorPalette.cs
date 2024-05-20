using SixLabors.ImageSharp;

namespace MapToolkit.Drawing.Topographic
{
    internal class ColorPalette
    {
        internal static readonly ColorPalette Default = new ColorPalette();

        public Color ContourMinor { get; set; } = Color.ParseHex("D4C5BF");
        public Color ContourMajor { get; set; } = Color.ParseHex("B29A94");
        public Color ForestFill { get; set; } = Color.ParseHex("D1FEB9");
        public Color ForestBorder { get; set; } = Color.ParseHex("8AE854");
        public Color WaterFill { get; set; } = Color.ParseHex("B3D9FE");
        public Color WaterBorder { get; set; } = Color.ParseHex("77A5E1");
        public Color BuildingFill { get; set; } = Color.ParseHex("808080");
        public Color BuildingBorder { get; set; } = Color.ParseHex("000000");
        public Color RocksSymbol { get; set; } = Color.ParseHex("C0C0C0");
        public Color RocksFill { get; set; } = Color.ParseHex("C0C0C080");
        public Color RocksBorder { get; set; } = Color.ParseHex("808080");
        public Color MainRoad { get; set; } = Color.ParseHex("FE002C");
        public Color SecondaryRoad { get; set; } = Color.ParseHex("FF9643");
        public Color Road { get; set; } = Color.White;
        public Color RoadBorder { get; set; } = Color.Black;
        public Color Trail { get; set; } = Color.ParseHex("C0C0C0");
        public Color Graticule { get; set; } = Color.ParseHex("0071D6");
        public Color TextBorder { get; set; } = Color.ParseHex("FFFFFFCC");

    }
}
