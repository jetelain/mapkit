using Xunit;
using Pmad.Cartography.GeodeticSystems;
using System;

namespace Pmad.Cartography.Test.GeodeticSystems
{
    public class WSG84ApproximateDistanceTest
    {
        [Fact]
        public void DistanceInMeters_SameCoordinates_ReturnsZero()
        {
            var coord = new Coordinates(0, 0);
            var distance = WSG84ApproximateDistance.Instance.DistanceInMeters(coord, coord);
            Assert.Equal(0, distance);
        }

        [Fact]
        public void DistanceInMeters_DifferentCoordinates_ReturnsCorrectDistance()
        {
            var coord1 = new Coordinates(48.8566, 2.3522); // Paris
            var coord2 = new Coordinates(51.5074, -0.1278); // London
            var distance = WSG84ApproximateDistance.Instance.DistanceInMeters(coord1, coord2);
            Assert.InRange(distance, 340_000, 350_000); // Approximate distance in meters
        }
    }
}
