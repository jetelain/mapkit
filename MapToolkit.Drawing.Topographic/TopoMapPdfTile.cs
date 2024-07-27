namespace MapToolkit.Drawing.Topographic
{
    internal class TopoMapPdfTile
    {
        public TopoMapPdfTile(string name, CoordinatesS min, CoordinatesS max)
        {
            Name = name;
            Min = min;
            Max = max;
        }

        public string Name { get; }
        public CoordinatesS Min { get; }
        public CoordinatesS Max { get; }
    }
}