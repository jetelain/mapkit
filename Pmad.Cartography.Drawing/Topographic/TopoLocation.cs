namespace Pmad.Cartography.Drawing.Topographic
{
    public sealed class TopoLocation
    {
        public TopoLocation(string name, TopoLocationType type, CoordinatesValue position)
        {
            Name = name;
            Type = type;
            Position = position;
        }

        public string Name { get; }

        public TopoLocationType Type { get; }

        public CoordinatesValue Position { get; }
    }
}