using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDEM.GeodeticSystems
{
    public class WSG84
    {
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

        public static double EPow2 = 0.006_694_379_990_141_317; // == (Math.Pow(A, 2) - Math.Pow(B, 2)) / Math.Pow(A, 2);

        public static double E = 0.081_819_190_842_622; // == Math.Sqrt(EPow2);

        private const double PIDiv180 = Math.PI / 180;

        // A - (F * A) = B


        /// <summary>
        /// Length of a degree of longitude (east–west distance)
        /// </summary>
        /// <param name="lat">Latitude (-90 to +90)</param>
        /// <returns>Length of a degree of longitude in meters at <paramref name="lat"/> longitude.</returns>
        public static double Delta1Long(double lat)
        {
            // https://en.wikipedia.org/wiki/Longitude#Length_of_a_degree_of_longitude
            var ϕ = lat * PIDiv180;
            // return (a * Math.Cos(ϕ)) / (Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(ϕ), 2))) * PIDiv180;
            // Use alternative formula, because it's 20% faster
            return PIDiv180 * A * Math.Cos(Math.Atan(B / A * Math.Tan(ϕ)));
        }

        /// <summary>
        /// Meridian distance
        /// </summary>
        /// <param name="lat"></param>
        /// <returns>The distance in metres between latitudes <paramref name="lat"/> − 0.5 degrees and <paramref name="lat"/> + 0.5 degrees</returns>
        public static double Delta1Lat(double lat)
        {
            // https://en.wikipedia.org/wiki/Latitude#Meridian_distance_on_the_ellipsoid
            var ϕ = lat * PIDiv180;
            return 111132.954 - 559.822 * Math.Cos(2 * ϕ) + 1.175 * Math.Cos(4 * ϕ);
        }
    }
}
