using System;
using System.Collections.Generic;
using System.Text;

namespace Pmad.Cartography.GeodeticSystems
{
    public interface IDistanceAlgorithm
    {
        double DistanceInMeters(Coordinates a, Coordinates b);
    }
}
