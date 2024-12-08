using Xunit;
using Pmad.Cartography.GeodeticSystems;

namespace Pmad.Cartography.Test.GeodeticSystems
{
    public class MeterProjectedDistanceTest
    {
        [Fact]
        public void DistanceInMeters_SameCoordinates_ReturnsZero()
        {
            var coordinates = new Coordinates(0, 0);
            var distance = MeterProjectedDistance.Instance.DistanceInMeters(coordinates, coordinates);
            Assert.Equal(0, distance);
        }

        [Fact]
        public void DistanceInMeters_DifferentCoordinates_ReturnsCorrectDistance()
        {
            var coordinates1 = new Coordinates(0, 0);
            var coordinates2 = new Coordinates(0, 1);
            var distance = MeterProjectedDistance.Instance.DistanceInMeters(coordinates1, coordinates2);
            Assert.Equal(1, distance);
        }
    }
}
