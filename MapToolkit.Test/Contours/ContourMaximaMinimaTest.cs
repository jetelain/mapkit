using System;
using System.Collections.Generic;
using System.Linq;
using Pmad.Cartography.Contours;
using Pmad.Cartography.DataCells;
using Pmad.Geometry;
using Pmad.Geometry.Collections;
using Xunit;

namespace Pmad.Cartography.Test.Contours
{
    public class ContourMaximaMinimaTest
    {
        [Fact]
        public void FindMinima_SingleMinima()
        {
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 10, 10, 10 },
                { 10, 0, 10 },
                { 10, 10, 10 }
            });

            var lines = new List<ContourLine>
            {
                new ContourLine(new List<CoordinatesValue>
                {
                    new CoordinatesValue(0.25, 0.25),
                    new CoordinatesValue(0.75, 0.25),
                    new CoordinatesValue(0.75, 0.75),
                    new CoordinatesValue(0.25, 0.75),
                    new CoordinatesValue(0.25, 0.25)
                }, 15)
            };

            var minima = ContourMaximaMinima.FindMinima(cell, lines);
            Assert.False(lines[0].IsCounterClockWise);
            var point = Assert.Single(minima);
            Assert.Equal(0, point.Elevation);
            Assert.Equal(0.5, point.Latitude);
            Assert.Equal(0.5, point.Longitude);
        }

        [Fact]
        public void FindMaxima_SingleMaxima()
        {
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 0, 10, 0 },
                { 0, 0, 0 }
            });

            var lines = new List<ContourLine>
            {
                new ContourLine(new List<CoordinatesValue>
                {
                    new CoordinatesValue(0.25, 0.25),
                    new CoordinatesValue(0.25, 0.75),
                    new CoordinatesValue(0.75, 0.75),
                    new CoordinatesValue(0.75, 0.25),
                    new CoordinatesValue(0.25, 0.25)
                }, 5)
            };

            var maxima = ContourMaximaMinima.FindMaxima(cell, lines);
            Assert.True(lines[0].IsCounterClockWise);
            var point = Assert.Single(maxima);
            Assert.Equal(10, point.Elevation);
            Assert.Equal(0.5, point.Latitude);
            Assert.Equal(0.5, point.Longitude);
        }

        [Fact]
        public void FindMinima_NoMinima()
        {
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 10, 10, 10 },
                { 10, 10, 10 },
                { 10, 10, 10 }
            });

            var lines = new List<ContourLine>
            {
                new ContourLine(new List<CoordinatesValue>
                {
                    new CoordinatesValue(0, 0),
                    new CoordinatesValue(1, 0),
                    new CoordinatesValue(1, 1),
                    new CoordinatesValue(0, 1),
                    new CoordinatesValue(0, 0)
                }, 5)
            };

            var minima = ContourMaximaMinima.FindMinima(cell, lines);
            Assert.False(lines[0].IsCounterClockWise);
            Assert.Empty(minima);
        }

        [Fact]
        public void FindMaxima_NoMaxima()
        {
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 0, 0, 0 },
                { 0, 0, 0 }
            });

            var lines = new List<ContourLine>
            {
                new ContourLine(new List<CoordinatesValue>
                {
                    new CoordinatesValue(0, 0),
                    new CoordinatesValue(0, 1),
                    new CoordinatesValue(1, 1),
                    new CoordinatesValue(1, 0),
                    new CoordinatesValue(0, 0)
                }, 5)
            };

            var maxima = ContourMaximaMinima.FindMaxima(cell, lines);
            Assert.True(lines[0].IsCounterClockWise);
            Assert.Empty(maxima);
        }
    }
}
