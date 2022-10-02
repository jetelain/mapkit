using System;

namespace MapToolkit.GeodeticSystems
{
    public class WSG84
    {
        public const ushort EPSG = 4326;

        // https://fr.wikipedia.org/wiki/WGS_84
        // https://en.wikipedia.org/wiki/World_Geodetic_System#WGS84

        /// <summary>
        /// Semi-major axis a
        /// </summary>
        public const double A = 6_378_137.0;

        /// <summary>
        /// Inverse flattening
        /// </summary>
        public const double InvF = 298.257_223_563;

        // --- Computed ---

        /// <summary>
        /// Semi-minor axis b
        /// </summary>
        public const double B = 6_356_752.314_245_179; // == A - (A / InvF);

        public const double EPow2 = 0.006_694_379_990_141_317; // == (Math.Pow(A, 2) - Math.Pow(B, 2)) / Math.Pow(A, 2);

        public const double E = 0.081_819_190_842_622; // == Math.Sqrt(EPow2);

        public const double R = 6_371_008.771_415_059; // == (2 * A + B) / 3

        /// <summary>
        /// Length of a degree of longitude (east–west distance)
        /// </summary>
        /// <param name="lat">Latitude (-90 to +90)</param>
        /// <returns>Length of a degree of longitude in meters at <paramref name="lat"/> longitude.</returns>
        public static double Delta1Long(double lat)
        {
            // https://en.wikipedia.org/wiki/Longitude#Length_of_a_degree_of_longitude
            var ϕ = lat * MathConstants.PIDiv180;
            // Use alternative formula, because it's 20% faster
            return MathConstants.PIDiv180 * A * Math.Cos(Math.Atan(B / A * Math.Tan(ϕ)));
        }

        /// <summary>
        /// Meridian distance
        /// </summary>
        /// <param name="lat"></param>
        /// <returns>The distance in metres between latitudes <paramref name="lat"/> − 0.5 degrees and <paramref name="lat"/> + 0.5 degrees</returns>
        public static double Delta1Lat(double lat)
        {
            // https://en.wikipedia.org/wiki/Latitude#Meridian_distance_on_the_ellipsoid
            var ϕ = lat * MathConstants.PIDiv180;
            return 111132.954 - 559.822 * Math.Cos(2 * ϕ) + 1.175 * Math.Cos(4 * ϕ);
        }

        /// <summary>
        /// Distance according to Haversine formula.
        /// 
        /// For long distances : precision is about 0.1%, but is 0.5% in the worst case
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double ApproximateDistance(Coordinates a, Coordinates b)
        {
            var aLat = a.Latitude * MathConstants.PIDiv180;
            var aLon = a.Longitude * MathConstants.PIDiv180;
            var bLat = b.Latitude * MathConstants.PIDiv180;
            var bLon = b.Longitude * MathConstants.PIDiv180;
            var x = Math.Pow(Math.Sin((bLat - aLat) / 2.0), 2.0) + Math.Cos(aLat) * Math.Cos(bLat) * Math.Pow(Math.Sin((bLon - aLon) / 2.0), 2.0);
            return R * (2.0 * Math.Atan2(Math.Sqrt(x), Math.Sqrt(1.0 - x)));
        }

        public static double Heading(Coordinates a, Coordinates b)
        {
            var aLat = a.Latitude * MathConstants.PIDiv180;
            var aLon = a.Longitude * MathConstants.PIDiv180;
            var bLat = b.Latitude * MathConstants.PIDiv180;
            var bLon = b.Longitude * MathConstants.PIDiv180;
            var dL = bLon - aLon;
            return Math.Atan2(Math.Cos(bLat) * Math.Sin(dL), Math.Cos(aLat) * Math.Sin(bLat) - Math.Sin(aLat) * Math.Cos(bLat) * Math.Cos(dL));
        }
    }
}
