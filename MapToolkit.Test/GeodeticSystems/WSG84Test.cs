using System;
using Pmad.Cartography.GeodeticSystems;
using Xunit;

namespace Pmad.Cartography.Test.GeodeticSystems
{
    public class WSG84Test
    {
        [Fact]
        public void Delta1Long()
        {
            // Test at equator
            double result = WSG84.Delta1Long(0);
            Assert.Equal(111319.4907, result, 3);

            // Test at 45 degrees latitude
            result = WSG84.Delta1Long(45);
            Assert.Equal(78846.8350, result, 3);

            // Test at 90 degrees latitude
            result = WSG84.Delta1Long(90);
            Assert.Equal(0, result, 5);
        }

        [Fact]
        public void Delta1Lat()
        {
            // Test at equator
            double result = WSG84.Delta1Lat(0);
            Assert.Equal(110574.307, result, 3);

            // Test at 45 degrees latitude
            result = WSG84.Delta1Lat(45);
            Assert.Equal(111131.7789, result, 3);

            // Test at 90 degrees latitude
            result = WSG84.Delta1Lat(90);
            Assert.Equal(111693.951, result, 3);
        }

        [Fact]
        public void ApproximateDistance()
        {
            var coord1 = new Coordinates(0, 0);
            var coord2 = new Coordinates(0, 1);
            double result = WSG84.ApproximateDistance(coord1, coord2);
            Assert.Equal(111195.0797, result, 3);

            coord1 = new Coordinates(0, 0);
            coord2 = new Coordinates(1, 0);
            result = WSG84.ApproximateDistance(coord1, coord2);
            Assert.Equal(111195.0797, result, 3);

            coord1 = new Coordinates(0, 0);
            coord2 = new Coordinates(1, 1);
            result = WSG84.ApproximateDistance(coord1, coord2);
            Assert.Equal(157249.5977, result, 3);
        }

        [Fact]
        public void Heading()
        {
            var coord1 = new Coordinates(0, 0);
            var coord2 = new Coordinates(0, 1);
            double result = WSG84.Heading(coord1, coord2);
            Assert.Equal(1.5708, result, 4);

            coord1 = new Coordinates(0, 0);
            coord2 = new Coordinates(1, 0);
            result = WSG84.Heading(coord1, coord2);
            Assert.Equal(0, result, 4);

            coord1 = new Coordinates(0, 0);
            coord2 = new Coordinates(1, 1);
            result = WSG84.Heading(coord1, coord2);
            Assert.Equal(0.7853, result, 4);
        }
    }
}
