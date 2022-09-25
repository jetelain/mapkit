using System;
using System.Collections.Generic;
using System.Text;

namespace MapToolkit.GeodeticSystems
{
    public sealed class WSG84ApproximateDistance : IDistanceAlgorithm
    {
        public static readonly IDistanceAlgorithm Instance = new WSG84ApproximateDistance();

        public double DistanceInMeters(Coordinates a, Coordinates b)
        {
            return WSG84.ApproximateDistance(a, b);
        }
    }
}
