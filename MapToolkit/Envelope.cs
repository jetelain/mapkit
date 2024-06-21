using GeoJSON.Text.Geometry;

namespace MapToolkit
{
    internal sealed class Envelope
    {
        internal static readonly Envelope None = new Envelope(new Coordinates(double.MinValue, double.MinValue), new Coordinates(double.MaxValue, double.MaxValue));

        public Envelope(IPosition minPoint, IPosition maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }

        public IPosition MinPoint { get; }

        public IPosition MaxPoint { get; }

        internal bool Intersects(Envelope item)
        {
            return item.MinPoint.Longitude <= MaxPoint.Longitude &&
                item.MinPoint.Latitude <= MaxPoint.Latitude &&
                item.MaxPoint.Longitude >= MinPoint.Longitude &&
                item.MaxPoint.Latitude >= MinPoint.Latitude;
        }

        internal bool Contains(Envelope item)
        {
            return
                item.MinPoint.Longitude >= MinPoint.Longitude &&
                item.MinPoint.Latitude >= MinPoint.Latitude &&
                item.MaxPoint.Longitude <= MaxPoint.Longitude &&
                item.MaxPoint.Latitude <= MaxPoint.Latitude;
        }
    }
}
