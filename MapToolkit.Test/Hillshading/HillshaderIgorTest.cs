using Pmad.Cartography.DataCells;
using Pmad.Cartography.Hillshading;

namespace Pmad.Cartography.Test.Hillshading
{
    public class HillshaderIgorTest
    {
        [Fact]
        public void GetPixelLuminance_FlatTerrain()
        {
            var hillshader = new HillshaderIgor();
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 }
            });
            var luminance = hillshader.GetPixels(cell);
            Assert.Equal(255, luminance[0, 0].PackedValue);
        }

        [Fact]
        public void GetPixelLuminance_Negative()
        {
            var hillshader = new HillshaderIgor();
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 10, 10, 10 },
                { 0, 0, 0 },
                { 10, 10, 10 }
            });
            var luminance = hillshader.GetPixels(cell);
            Assert.Equal(87, luminance[0, 0].PackedValue);
        }

        [Fact]
        public void GetPixelLuminance_Positive()
        {
            var hillshader = new HillshaderIgor();
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 10, 10, 10 },
                { 0, 0, 0 }
            });
            var luminance = hillshader.GetPixels(cell);
            Assert.Equal(199, luminance[0, 0].PackedValue);
        }

        [Fact]
        public void NormalizeAngle()
        {
            Assert.Equal(10, HillshaderIgor.NormalizeAngle(370, 360));
            Assert.Equal(350, HillshaderIgor.NormalizeAngle(-10, 360));
        }

        [Fact]
        public void DifferenceBetweenAngles()
        {
            Assert.Equal(30, HillshaderIgor.DifferenceBetweenAngles(30, 60, 360));
            Assert.Equal(30, HillshaderIgor.DifferenceBetweenAngles(60, 30, 360));
        }
    }
}
