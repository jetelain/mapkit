namespace Pmad.Cartography.Drawing.Topographic
{
    internal class TopoMapPdfTile
    {
        public TopoMapPdfTile(string name, CoordinatesValue min, CoordinatesValue max)
        {
            Name = name;
            Min = min;
            Max = max;
        }

        public string Name { get; }
        public CoordinatesValue Min { get; }
        public CoordinatesValue Max { get; }
    }
}