using System;
using Pmad.Cartography.DataCells;
using Xunit;

namespace Pmad.Cartography.Test.DataCells
{
    public class RasterMappingTest
    {
        [Fact]
        public void Create_RasterPixelIsArea_WithPoints()
        {
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            var raster = RasterMapping.Create(DemRasterType.PixelIsArea, start, end, 100, 100);

            Assert.IsType<RasterPixelIsArea>(raster);
            Assert.Equal(start, raster.Start);
            Assert.Equal(end, raster.End);
            Assert.Equal(100, raster.PointsLat);
            Assert.Equal(100, raster.PointsLon);
        }

        [Fact]
        public void Create_RasterPixelIsPoint_WithPoints()
        {
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            var raster = RasterMapping.Create(DemRasterType.PixelIsPoint, start, end, 100, 100);

            Assert.IsType<RasterPixelIsPoint>(raster);
            Assert.Equal(start, raster.Start);
            Assert.Equal(end, raster.End);
            Assert.Equal(100, raster.PointsLat);
            Assert.Equal(100, raster.PointsLon);
        }

        [Fact]
        public void Create_RasterPixelIsArea_WithPixelSize()
        {
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            var raster = RasterMapping.Create(DemRasterType.PixelIsArea, start, end, 0.1, 0.1);

            Assert.IsType<RasterPixelIsArea>(raster);
            Assert.Equal(start, raster.Start);
            Assert.Equal(end, raster.End);
            Assert.Equal(0.1, raster.PixelSizeLat);
            Assert.Equal(0.1, raster.PixelSizeLon);
        }

        [Fact]
        public void Create_RasterPixelIsPoint_WithPixelSize()
        {
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            var raster = RasterMapping.Create(DemRasterType.PixelIsPoint, start, end, 0.1, 0.1);

            Assert.IsType<RasterPixelIsPoint>(raster);
            Assert.Equal(start, raster.Start);
            Assert.Equal(end, raster.End);
            Assert.Equal(0.1, raster.PixelSizeLat);
            Assert.Equal(0.1, raster.PixelSizeLon);
        }

        [Fact]
        public void PinToGridCeiling_Test()
        {
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            var raster = RasterMapping.Create(DemRasterType.PixelIsArea, start, end, 0.1, 0.1);
            var coordinates = new Coordinates(0.15, 0.15);
            var result = raster.PinToGridCeiling(coordinates);

            Assert.Equal(new Coordinates(0.2, 0.2), result);
        }

        [Fact]
        public void PinToGridFloor_Test()
        {
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            var raster = RasterMapping.Create(DemRasterType.PixelIsArea, start, end, 0.1, 0.1);
            var coordinates = new Coordinates(0.15, 0.15);
            var result = raster.PinToGridFloor(coordinates);

            Assert.Equal(new Coordinates(0.1, 0.1), result);
        }

        [Fact]
        public void Crop_Test()
        {
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            var raster = RasterMapping.Create(DemRasterType.PixelIsArea, start, end, 0.1, 0.1);
            var wantedStart = new Coordinates(1.15, 1.15);
            var wantedEnd = new Coordinates(8.85, 8.85);
            var croppedRaster = raster.Crop(wantedStart, wantedEnd);

            Assert.Equal(new Coordinates(1.1, 1.1), croppedRaster.Start);
            Assert.Equal(new Coordinates(8.9, 8.9), croppedRaster.End);
        }
    }
}
