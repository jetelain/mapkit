using System;
using System.Linq;
using Pmad.Cartography.DataCells;
using Pmad.Cartography.Hillshading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Pmad.Cartography.Test.Hillshading
{
    public class HillshaderBaseTest
    {
        private class TestHillshader : HillshaderBase
        {
            protected override double Flat => 0.5;

            protected override double GetPixelLuminance(double[] southLine, double[] line, double[] northLine, int x)
            {
                return line[x] / 10.0;
            }
        }

        [Fact]
        public void GetPixels_ReturnsCorrectImage()
        {
            var hillshader = new TestHillshader();
            var cell = new DemDataCellPixelIsPoint<double>( new Coordinates(0,0), new Coordinates(1,1), new double[,] {
                    { 0, 0, 0 },
                    { 0, 10, 0 },
                    { 0, 0, 0 }
                });
            var image = hillshader.GetPixels(cell);

            Assert.Equal(3, image.Width);
            Assert.Equal(3, image.Height);
            Assert.Equal(new L8(0), image[0, 0]);
            Assert.Equal(new L8(255), image[1, 1]);
        }

        [Fact]
        public void GetPixelsBelowFlat_ReturnsCorrectImage()
        {
            var hillshader = new TestHillshader();
            var cell = new DemDataCellPixelIsPoint<double>(new Coordinates(0, 0), new Coordinates(1, 1), new double[,] {
                    { 0, 0, 0 },
                    { 0, 10, 0 },
                    { 0, 0, 0 }
                });
            var image = hillshader.GetPixelsBelowFlat(cell);

            Assert.Equal(3, image.Width);
            Assert.Equal(3, image.Height);
            Assert.Equal(new L8(0), image[0, 0]);
            Assert.Equal(new L8(255), image[1, 1]);
        }

        [Fact]
        public void GetPixelsAlpha_ReturnsCorrectImage()
        {
            var hillshader = new TestHillshader();
            var cell = new DemDataCellPixelIsPoint<double>(new Coordinates(0, 0), new Coordinates(1, 1), new double[,] {
                    { 0, 0, 0 },
                    { 0, 10, 0 },
                    { 0, 0, 0 }
                }); 
            var image = hillshader.GetPixelsAlpha(cell);

            Assert.Equal(3, image.Width);
            Assert.Equal(3, image.Height);
            Assert.Equal(new La16(0, 255), image[0, 0]);
            Assert.Equal(new La16(0, 0), image[1, 1]);
        }

        [Fact]
        public void GetPixelsAlphaBelowFlat_ReturnsCorrectImage()
        {
            var hillshader = new TestHillshader();
            var cell = new DemDataCellPixelIsPoint<double>(new Coordinates(0, 0), new Coordinates(1, 1), new double[,] {
                    { 0, 0, 0 },
                    { 0, 10, 0 },
                    { 0, 0, 0 }
                });
            var image = hillshader.GetPixelsAlphaBelowFlat(cell);

            Assert.Equal(3, image.Width);
            Assert.Equal(3, image.Height);
            Assert.Equal(new La16(0, 255), image[0, 0]);
            Assert.Equal(new La16(0, 0), image[1, 1]);
        }
    }
}
