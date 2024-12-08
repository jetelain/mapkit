using System.Collections.Generic;
using Xunit;
using Pmad.Cartography;

namespace Pmad.Cartography.Test
{
    public class TriangleNWToSEInterpolationTest
    {
        [Fact]
        public void Interpolate_TopLeftTriangle_ReturnsCorrectValue()
        {
            // Arrange
            double f00 = 0, f10 = 10, f01 = 5, f11 = 15;
            double x = 0.25, y = 0.25;
            var interpolation = TriangleNWToSEInterpolation.Instance;

            // Act
            double result = interpolation.Interpolate(f00, f10, f01, f11, x, y);

            // Assert
            Assert.Equal(3.75, result);
        }

        [Fact]
        public void Interpolate_BottomRightTriangle_ReturnsCorrectValue()
        {
            // Arrange
            double f00 = 0, f10 = 10, f01 = 5, f11 = 15;
            double x = 0.75, y = 0.75;
            var interpolation = TriangleNWToSEInterpolation.Instance;

            // Act
            double result = interpolation.Interpolate(f00, f10, f01, f11, x, y);

            // Assert
            Assert.Equal(11.25, result);
        }

        [Fact]
        public void Interpolate_Coordinates_ReturnsDefaultInterpolationValue()
        {
            // Arrange
            var coordinates = new Coordinates(0.5, 0.5);
            var points = new List<DemDataPoint>
            {
                new DemDataPoint(new Coordinates(0, 0), 0),
                new DemDataPoint(new Coordinates(1, 0), 10),
                new DemDataPoint(new Coordinates(0, 1), 5),
                new DemDataPoint(new Coordinates(1, 1), 15)
            };
            var interpolation = TriangleNWToSEInterpolation.Instance;

            // Act
            double result = interpolation.Interpolate(coordinates, points);

            // Assert
            Assert.Equal(DefaultInterpolation.Instance.Interpolate(coordinates, points), result);
        }
    }
}
