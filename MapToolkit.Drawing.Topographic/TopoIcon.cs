namespace MapToolkit.Drawing.Topographic
{
    public sealed class TopoIcon
    {
        public TopoIcon(TopoIconType mapType, Coordinates coordinates)
        {
            MapType = mapType;
            Coordinates = coordinates;
        }

        public TopoIconType MapType { get; }

        public Coordinates Coordinates { get; }
    }
}
