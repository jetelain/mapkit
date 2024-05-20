namespace MapToolkit.Drawing.Topographic
{
    internal class TopoMapPdfTile
    {
        public TopoMapPdfTile(string name, Coordinates min, Coordinates max)
        {
            Name = name;
            Min = min;
            Max = max;
        }

        public string Name { get; }
        public Coordinates Min { get; }
        public Coordinates Max { get; }
    }
}