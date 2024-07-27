namespace MapToolkit.Drawing.Topographic
{
    public sealed class TopoIcon
    {
        public TopoIcon(TopoIconType mapType, CoordinatesS coordinates)
        {
            MapType = mapType;
            Coordinates = coordinates;
        }

        public TopoIconType MapType { get; }

        public CoordinatesS Coordinates { get; }
    }
}
