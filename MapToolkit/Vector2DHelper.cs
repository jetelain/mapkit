using Pmad.Geometry;

namespace MapToolkit
{
    public static class Vector2DHelper
    {
        public static double Latitude(this Vector2D vector) => vector.Y;

        public static double Longitude(this Vector2D vector) => vector.X;

        public static Vector2D FromLatLon(double lat, double lon) => new Vector2D(lon, lat);

    }
}
