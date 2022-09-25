using System;
using System.Collections.Generic;
using System.Text;

namespace MapToolkit.GeodeticSystems
{
    public interface IDistanceAlgorithm
    {
        double DistanceInMeters(Coordinates a, Coordinates b);
    }
}
