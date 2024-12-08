using System;
using Pmad.Cartography.Projections;
using Pmad.Geometry;
using Xunit;

namespace Pmad.Cartography.Test.Projections
{
    public class NoProjectionAreaTest
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            var min = new Coordinates(0, 0);
            var max = new Coordinates(10, 10);
            var size = new Vector(100, 100);

            var projectionArea = new NoProjectionArea(min, max, size);

            Assert.Equal(size, projectionArea.Size);
            Assert.Equal(Vector.Zero, projectionArea.Min);
        }

        [Fact]
        public void Project_SinglePoint()
        {
            var min = new Coordinates(0, 0);
            var max = new Coordinates(10, 10);
            var size = new Vector(100, 100);
            var projectionArea = new NoProjectionArea(min, max, size);

            var point = new CoordinatesValue(5, 5);
            var projected = projectionArea.Project(point);

            Assert.Equal(new Vector(50, 50), projected);
        }

        [Fact]
        public void Project_MultiplePoints()
        {
            var min = new Coordinates(0, 0);
            var max = new Coordinates(10, 10);
            var size = new Vector(100, 100);
            var projectionArea = new NoProjectionArea(min, max, size);

            var points = new CoordinatesValue[]
            {
                new CoordinatesValue(5, 5),
                new CoordinatesValue(2.5, 2.5),
                new CoordinatesValue(7.5, 7.5)
            };

            var projected = projectionArea.Project(points);

            Assert.Equal(3, projected.Length);
            Assert.Equal(new Vector(50, 50), projected[0]);
            Assert.Equal(new Vector(25, 75), projected[1]);
            Assert.Equal(new Vector(75, 25), projected[2]);
        }
    }
}
