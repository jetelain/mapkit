﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.DataCells
{
    public class RasterPixelIsPointTest
    {

        [Fact]
        public void RasterPixelIsPoint_Ctor()
        {
            var raster = new RasterPixelIsPoint(new Coordinates(0, 0), new Coordinates(1, 1), 0.0041666666666666666, 0.0041666666666666666);
            Assert.Equal(241, raster.PointsLat);
            Assert.Equal(241, raster.PointsLon);
            Assert.Equal(0.0041666666666666666, raster.PixelSizeLat);
            Assert.Equal(0.0041666666666666666, raster.PixelSizeLon);

            raster = new RasterPixelIsPoint(new Coordinates(39.729166666667, 25.0125), new Coordinates(40.075, 25.4625), 0.0041666666666666666, 0.0041666666666666666);
            Assert.Equal(84, raster.PointsLat);
            Assert.Equal(109, raster.PointsLon);
            Assert.Equal(0.0041666666666666666, raster.PixelSizeLat);
            Assert.Equal(0.0041666666666666666, raster.PixelSizeLon);
        }

        [Fact]
        public void CoordinatesToIndexClosest()
        {
            var raster = new RasterPixelIsPoint(new Coordinates(0, 0), new Coordinates(1, 1), 0.5, 0.5);

            var p = raster.CoordinatesToIndexClosest(new Coordinates(-0.5, 0));
            Assert.Equal(-1, p.Latitude);
            Assert.Equal(0, p.Longitude); 
            
            p = raster.CoordinatesToIndexClosest(new Coordinates(0, 0));
            Assert.Equal(0, p.Latitude);
            Assert.Equal(0, p.Longitude);

            p = raster.CoordinatesToIndexClosest(new Coordinates(0.5, 0));
            Assert.Equal(1, p.Latitude);
            Assert.Equal(0, p.Longitude);

            p = raster.CoordinatesToIndexClosest(new Coordinates(1, 0));
            Assert.Equal(2, p.Latitude);
            Assert.Equal(0, p.Longitude);
            
            p = raster.CoordinatesToIndexClosest(new Coordinates(1.5, 0));
            Assert.Equal(3, p.Latitude);
            Assert.Equal(0, p.Longitude);
        }

    }
}
