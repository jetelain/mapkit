using System;
using Xunit;
using Pmad.Cartography.Projections;

namespace Pmad.Cartography.Test.Projections
{
    public class WebMercatorAreaTest
    {
        [Fact]
        public void Constructor_WithFullSizeAndRounding_SetsProperties()
        {
            var area = new WebMercatorArea(256, 2);
            Assert.Equal(new Vector(256, 256), area.Size);
        }

        [Fact]
        public void Constructor_WithFullSizeDXDYSizeAndRounding_SetsProperties()
        {
            var area = new WebMercatorArea(256, 1.5, 2.5, new Vector(512, 512), 3);
            Assert.Equal(new Vector(512, 512), area.Size);
        }

        [Fact]
        public void Min_ReturnsZeroVector()
        {
            var area = new WebMercatorArea(256);
            Assert.Equal(Vector.Zero, area.Min);
        }

        [Fact]
        public void Scale_ReturnsOne()
        {
            var area = new WebMercatorArea(256);
            Assert.Equal(1, area.Scale);
        }

        [Fact]
        public void Project_WithRounding_ProjectsCoordinates()
        {
            var area = new WebMercatorArea(256, 2);
            var point = new CoordinatesValue(45, 90);
            var result = area.Project(point);
            Assert.Equal(new Vector(92.09, 192), result);
        }

        [Fact]
        public void Project_WithoutRounding_ProjectsCoordinates()
        {
            var area = new WebMercatorArea(256);
            var point = new CoordinatesValue(45, 90);
            var result = area.Project(point);
            Assert.Equal(new Vector(92.08960945029247,192), result);
        }

        [Fact]
        public void Project_MultipleCoordinates_ProjectsAllCoordinates()
        {
            var area = new WebMercatorArea(256, 2);
            var points = new CoordinatesValue[]
            {
                new CoordinatesValue(45, 90),
                new CoordinatesValue(-45, -90)
            };
            var result = area.Project(points);
            Assert.Equal(2, result.Length);
            Assert.Equal(new Vector(92.09, 192), result[0]);
            Assert.Equal(new Vector(163.91, 64), result[1]);
        }
    }
}
