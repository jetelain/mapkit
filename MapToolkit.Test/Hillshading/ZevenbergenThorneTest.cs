using System;
using Xunit;
using Pmad.Cartography.Hillshading;

namespace Pmad.Cartography.Test.Hillshading
{
    public class ZevenbergenThorneTest
    {
        [Fact]
        public void GetDeltaUnscaled_CalculatesCorrectly()
        {
            var resolution = new Vector(1, 1);
            var zevenbergenThorne = new ZevenbergenThorne(resolution);

            double[] southLine = { 1, 2, 3 };
            double[] line = { 4, 5, 6 };
            double[] northLine = { 7, 8, 9 };
            int x = 1;

            zevenbergenThorne.GetDeltaUnscaled(southLine, line, northLine, x, out double dx, out double dy);

            Assert.Equal(2, dx);
            Assert.Equal(-6, dy);
        }

        [Fact]
        public void GetDeltaUnscaled_HandlesEdgeCases()
        {
            var resolution = new Vector(1, 1);
            var zevenbergenThorne = new ZevenbergenThorne(resolution);

            double[] southLine = { 1, 2, 3 };
            double[] line = { 4, 5, 6 };
            double[] northLine = { 7, 8, 9 };

            // Test for x = 0 (left edge)
            zevenbergenThorne.GetDeltaUnscaled(southLine, line, northLine, 0, out double dxLeft, out double dyLeft);
            Assert.Equal(1, dxLeft);
            Assert.Equal(-6, dyLeft);

            // Test for x = line.Length - 1 (right edge)
            zevenbergenThorne.GetDeltaUnscaled(southLine, line, northLine, line.Length - 1, out double dxRight, out double dyRight);
            Assert.Equal(1, dxRight);
            Assert.Equal(-6, dyRight);
        }
    }
}
