namespace MapToolkit.Drawing.Topographic
{
    public sealed class TopoIcon
    {
        public TopoIcon(TopoIconType mapType, CoordinatesValue coordinates)
        {
            MapType = mapType;
            Coordinates = coordinates;
        }

        public TopoIconType MapType { get; }

        public CoordinatesValue Coordinates { get; }
    }
}
