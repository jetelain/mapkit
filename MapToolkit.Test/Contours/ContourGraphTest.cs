using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapToolkit.Contours;
using MapToolkit.DataCells;

namespace MapToolkit.Test.Contours
{
    public class ContourGraphTest
    {
        [Fact]
        public void Add_SinglePoint()
        {
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 0, 10, 0 },
                { 0, 0, 0 }
            });

            var graph = new ContourGraph();
            graph.Add(cell, new ContourFixedLevel([5]), false, null);
            var line = Assert.Single(graph.Lines);
            Assert.Equal(5, line.Level);
            Assert.Equal("(0.5 0.75, 0.25 0.5, 0.5 0.25, 0.75 0.5, 0.5 0.75)", line.Points.ToGeoString());

        }

        [Fact]
        public void ToPolygons_SinglePoint()
        {
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 0, 10, 0 },
                { 0, 0, 0 }
            });

            var graph = new ContourGraph();
            graph.Add(cell, new ContourFixedLevel([5]), false, null);
            Assert.Single(graph.Lines);
            var polygons = graph.ToPolygons();
            var polygon = Assert.Single(polygons);
            Assert.Equal("POLYGON ((0.5 0.75, 0.25 0.5, 0.5 0.25, 0.75 0.5, 0.5 0.75))", polygon.ToGeoString());


            cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 10, 10, 10 },
                { 10, 0, 10 },
                { 10, 10, 10 }
            });

            graph = new ContourGraph();
            graph.Add(cell, new ContourFixedLevel([5]), false, null);
            Assert.Single(graph.Lines);
            polygons = graph.ToPolygons();
            polygon = Assert.Single(polygons);
            Assert.Equal("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0), (0.75 0.5, 0.5 0.25, 0.25 0.5, 0.5 0.75, 0.75 0.5))", polygon.ToGeoString());
        }

        [Fact]
        public void ToPolygonsReverse_SinglePoint()
        {
            var cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 0, 0, 0 },
                { 0, 10, 0 },
                { 0, 0, 0 }
            });

            var graph = new ContourGraph();
            graph.Add(cell, new ContourFixedLevel([5]), false, null);
            Assert.Single(graph.Lines);
            var polygons = graph.ToPolygonsReverse();
            var polygon = Assert.Single(polygons);
            Assert.Equal("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0), (0.5 0.75, 0.25 0.5, 0.5 0.25, 0.75 0.5, 0.5 0.75))", polygon.ToGeoString());

            cell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 10, 10, 10 },
                { 10, 0, 10 },
                { 10, 10, 10 }
            });

            graph = new ContourGraph();
            graph.Add(cell, new ContourFixedLevel([5]), false, null);
            Assert.Single(graph.Lines);
            polygons = graph.ToPolygonsReverse();
            polygon = Assert.Single(polygons);
            Assert.Equal("POLYGON ((0.75 0.5, 0.5 0.25, 0.25 0.5, 0.5 0.75, 0.75 0.5))", polygon.ToGeoString());
        }
    }
}
