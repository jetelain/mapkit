using System;
using Xunit;
using Pmad.Cartography.Hillshading;
using Pmad.Geometry;

namespace Pmad.Cartography.Test.Hillshading
{
    public class HornTest
    {
        [Fact]
        public void GetDeltaUnscaled_CalculatesCorrectly()
        {
            // Arrange
            var resolution = new Vector2D(1, 1);
            var horn = new Horn(resolution);
            double[] southLine = { 1, 2, 3 };
            double[] line = { 4, 5, 6 };
            double[] northLine = { 7, 8, 9 };
            int x = 1;

            // Act
            horn.GetDeltaUnscaled(southLine, line, northLine, x, out double dx, out double dy);

            // Assert
            Assert.Equal(8, dx);
            Assert.Equal(-24, dy);
        }

        [Fact]
        public void GetDeltaUnscaled_HandlesEdgeCases()
        {
            // Arrange
            var resolution = new Vector2D(1, 1);
            var horn = new Horn(resolution);
            double[] southLine = { 1, 2, 3 };
            double[] line = { 4, 5, 6 };
            double[] northLine = { 7, 8, 9 };

            // Act & Assert
            horn.GetDeltaUnscaled(southLine, line, northLine, 0, out double dx0, out double dy0);
            Assert.Equal(4, dx0);
            Assert.Equal(-24, dy0);

            horn.GetDeltaUnscaled(southLine, line, northLine, 2, out double dx2, out double dy2);
            Assert.Equal(4, dx2);
            Assert.Equal(-24, dy2);
        }
    }
}
