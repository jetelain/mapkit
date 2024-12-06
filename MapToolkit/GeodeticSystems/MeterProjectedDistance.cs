using System;
using System.Collections.Generic;
using System.Text;

namespace Pmad.Cartography.GeodeticSystems
{
    public sealed class MeterProjectedDistance : IDistanceAlgorithm
    {
        public static readonly IDistanceAlgorithm Instance = new MeterProjectedDistance();

        public double DistanceInMeters(Coordinates a, Coordinates b)
        {
            return a.Distance(b);
        }
    }
}
