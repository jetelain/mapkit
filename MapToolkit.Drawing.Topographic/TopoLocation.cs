namespace MapToolkit.Drawing.Topographic
{
    public sealed class TopoLocation
    {
        public TopoLocation(string name, TopoLocationType type, CoordinatesS position)
        {
            Name = name;
            Type = type;
            Position = position;
        }

        public string Name { get; }

        public TopoLocationType Type { get; }

        public CoordinatesS Position { get; }
    }
}