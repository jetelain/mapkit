using System;
using Xunit;
using Pmad.Cartography.Hillshading;
using Pmad.Geometry;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.Hillshading
{
    public class HillshaderClassicTest
    {
        [Fact]
        public void GetPixelLuminance_FlatTerrain()
        {
            var hillshader = new HillshaderClassic();
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 }
            });
            var luminance = hillshader.GetPixels(cell);
            Assert.Equal(146, luminance[0, 0].PackedValue);
        }

        [Fact]
        public void GetPixelLuminance_Negative()
        {
            var hillshader = new HillshaderClassic();
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 10, 10, 10 },
                { 0, 0, 0 },
                { 10, 10, 10 }
            });
            var luminance = hillshader.GetPixels(cell);
            Assert.Equal(0, luminance[0, 0].PackedValue);
        }

        [Fact]
        public void GetPixelLuminance_Positive()
        {
            var hillshader = new HillshaderClassic();
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 10, 10, 10 },
                { 0, 0, 0 }
            });
            var luminance = hillshader.GetPixels(cell);
            Assert.Equal(173, luminance[0, 0].PackedValue);
        }
    }
}
