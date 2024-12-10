using System;
using Xunit;
using Pmad.Cartography;
using Pmad.Geometry;

namespace Pmad.Cartography.Test
{
    public class CoordinatesTest
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            var coordinates = new Coordinates(10, 20);
            Assert.Equal(10, coordinates.Latitude);
            Assert.Equal(20, coordinates.Longitude);
        }

        [Fact]
        public void FromXY_ShouldCreateCoordinates()
        {
            var coordinates = Coordinates.FromXY(20, 10);
            Assert.Equal(10, coordinates.Latitude);
            Assert.Equal(20, coordinates.Longitude);
        }

        [Fact]
        public void FromLatLon_ShouldCreateCoordinates()
        {
            var coordinates = Coordinates.FromLatLon(10, 20);
            Assert.Equal(10, coordinates.Latitude);
            Assert.Equal(20, coordinates.Longitude);
        }

        [Fact]
        public void Equals_ShouldReturnTrueForEqualCoordinates()
        {
            var coordinates1 = new Coordinates(10, 20);
            var coordinates2 = new Coordinates(10, 20);
            Assert.True(coordinates1.Equals(coordinates2));
        }

        [Fact]
        public void Equals_ShouldReturnFalseForDifferentCoordinates()
        {
            var coordinates1 = new Coordinates(10, 20);
            var coordinates2 = new Coordinates(20, 10);
            Assert.False(coordinates1.Equals(coordinates2));
        }

        [Fact]
        public void GetHashCode_ShouldReturnSameHashCodeForEqualCoordinates()
        {
            var coordinates1 = new Coordinates(10, 20);
            var coordinates2 = new Coordinates(10, 20);
            Assert.Equal(coordinates1.GetHashCode(), coordinates2.GetHashCode());
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            var coordinates = new Coordinates(10, 20);
            Assert.Equal("(10;20)", coordinates.ToString());
        }

        [Fact]
        public void Distance_ShouldReturnCorrectDistance()
        {
            var coordinates1 = new Coordinates(0, 0);
            var coordinates2 = new Coordinates(3, 4);
            Assert.Equal(5, coordinates1.Distance(coordinates2));
        }

        [Fact]
        public void IsInSquare_ShouldReturnTrueIfInSquare()
        {
            var coordinates = new Coordinates(5, 5);
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            Assert.True(coordinates.IsInSquare(start, end));
        }

        [Fact]
        public void IsInSquare_ShouldReturnFalseIfNotInSquare()
        {
            var coordinates = new Coordinates(15, 15);
            var start = new Coordinates(0, 0);
            var end = new Coordinates(10, 10);
            Assert.False(coordinates.IsInSquare(start, end));
        }

        [Fact]
        public void OperatorPlus_ShouldReturnCorrectCoordinates()
        {
            var coordinates = new Coordinates(10, 20);
            var vector = new Vector2D(5, 5);
            var result = coordinates + vector;
            Assert.Equal(15, result.Latitude);
            Assert.Equal(25, result.Longitude);
        }

        [Fact]
        public void OperatorMinus_ShouldReturnCorrectCoordinates()
        {
            var coordinates = new Coordinates(10, 20);
            var vector = new Vector2D(5, 5);
            var result = coordinates - vector;
            Assert.Equal(5, result.Latitude);
            Assert.Equal(15, result.Longitude);
        }

        [Fact]
        public void Round_ShouldReturnRoundedCoordinates()
        {
            var coordinates = new Coordinates(10.12345, 20.6789);
            var result = coordinates.Round(2);
            Assert.Equal(10.12, result.Latitude);
            Assert.Equal(20.68, result.Longitude);
        }
    }
}
