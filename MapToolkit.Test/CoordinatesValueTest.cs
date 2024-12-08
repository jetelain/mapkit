using System;
using Pmad.Cartography;
using Pmad.Geometry;
using Xunit;

namespace Pmad.Cartography.Test
{
    public class CoordinatesValueTest
    {
        [Fact]
        public void Constructor_WithLatitudeLongitude_ShouldSetProperties()
        {
            var coordinates = new CoordinatesValue(10.0, 20.0);
            Assert.Equal(10.0, coordinates.Latitude);
            Assert.Equal(20.0, coordinates.Longitude);
        }

        [Fact]
        public void Constructor_WithVector2D_ShouldSetProperties()
        {
            var vector = new Vector2D(20.0, 10.0);
            var coordinates = new CoordinatesValue(vector);
            Assert.Equal(10.0, coordinates.Latitude);
            Assert.Equal(20.0, coordinates.Longitude);
        }

        [Fact]
        public void Constructor_WithCoordinates_ShouldSetProperties()
        {
            var vector = new Vector2D(20.0, 10.0);
            var coordinates = new Coordinates(vector);
            var coordinatesValue = new CoordinatesValue(coordinates);
            Assert.Equal(10.0, coordinatesValue.Latitude);
            Assert.Equal(20.0, coordinatesValue.Longitude);
        }

        [Fact]
        public void AlmostEquals_ShouldReturnTrue_WhenCoordinatesAreEqual()
        {
            var coordinates1 = new CoordinatesValue(10.0, 20.0);
            var coordinates2 = new CoordinatesValue(10.0, 20.0);
            Assert.True(coordinates1.AlmostEquals(coordinates2, 0.0));
        }

        [Fact]
        public void AlmostEquals_ShouldReturnTrue_WhenCoordinatesAreWithinThreshold()
        {
            var coordinates1 = new CoordinatesValue(10.0, 20.0);
            var coordinates2 = new CoordinatesValue(10.1, 20.1);
            Assert.True(coordinates1.AlmostEquals(coordinates2, 0.1));
        }

        [Fact]
        public void AlmostEquals_ShouldReturnFalse_WhenCoordinatesAreOutsideThreshold()
        {
            var coordinates1 = new CoordinatesValue(10.0, 20.0);
            var coordinates2 = new CoordinatesValue(10.1, 20.1);
            Assert.False(coordinates1.AlmostEquals(coordinates2, 0.01));
        }

        [Fact]
        public void IsInSquare_WithVectorEnvelope_ShouldReturnTrue_WhenInside()
        {
            var coordinates = new CoordinatesValue(10.0, 20.0);
            var range = new VectorEnvelope<Vector2D>(new Vector2D(15.0, 5.0), new Vector2D(25.0, 15.0));
            Assert.True(coordinates.IsInSquare(range));
        }

        [Fact]
        public void IsInSquare_WithVectorEnvelope_ShouldReturnFalse_WhenOutside()
        {
            var coordinates = new CoordinatesValue(10.0, 20.0);
            var range = new VectorEnvelope<Vector2D>(new Vector2D(25.0, 15.0), new Vector2D(35.0, 25.0));
            Assert.False(coordinates.IsInSquare(range));
        }

        [Fact]
        public void IsInSquare_WithMinMax_ShouldReturnTrue_WhenInside()
        {
            var coordinates = new CoordinatesValue(10.0, 20.0);
            var min = new CoordinatesValue(5.0, 15.0);
            var max = new CoordinatesValue(15.0, 25.0);
            Assert.True(coordinates.IsInSquare(min, max));
        }

        [Fact]
        public void IsInSquare_WithMinMax_ShouldReturnFalse_WhenOutside()
        {
            var coordinates = new CoordinatesValue(10.0, 20.0);
            var min = new CoordinatesValue(15.0, 25.0);
            var max = new CoordinatesValue(25.0, 35.0);
            Assert.False(coordinates.IsInSquare(min, max));
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenCoordinatesAreEqual()
        {
            var coordinates1 = new CoordinatesValue(10.0, 20.0);
            var coordinates2 = new CoordinatesValue(10.0, 20.0);
            Assert.True(coordinates1.Equals(coordinates2));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenCoordinatesAreNotEqual()
        {
            var coordinates1 = new CoordinatesValue(10.0, 20.0);
            var coordinates2 = new CoordinatesValue(20.0, 30.0);
            Assert.False(coordinates1.Equals(coordinates2));
        }

        [Fact]
        public void GetHashCode_ShouldReturnSameHashCode_ForEqualCoordinates()
        {
            var coordinates1 = new CoordinatesValue(10.0, 20.0);
            var coordinates2 = new CoordinatesValue(10.0, 20.0);
            Assert.Equal(coordinates1.GetHashCode(), coordinates2.GetHashCode());
        }

        [Fact]
        public void OperatorPlus_ShouldReturnCorrectSum()
        {
            var coordinates = new CoordinatesValue(10.0, 20.0);
            var vector = new Vector(5.0, 5.0);
            var result = coordinates + vector;
            Assert.Equal(15.0, result.Latitude);
            Assert.Equal(25.0, result.Longitude);
        }

        [Fact]
        public void OperatorMinus_ShouldReturnCorrectDifference()
        {
            var coordinates = new CoordinatesValue(10.0, 20.0);
            var vector = new Vector(5.0, 5.0);
            var result = coordinates - vector;
            Assert.Equal(5.0, result.Latitude);
            Assert.Equal(15.0, result.Longitude);
        }

        [Fact]
        public void OperatorMinus_ShouldReturnCorrectVectorDifference()
        {
            var coordinates1 = new CoordinatesValue(10.0, 20.0);
            var coordinates2 = new CoordinatesValue(5.0, 15.0);
            var result = coordinates1 - coordinates2;
            Assert.Equal(5.0, result.DeltaLat);
            Assert.Equal(5.0, result.DeltaLon);
        }

        [Fact]
        public void ImplicitConversion_ToCoordinates_ShouldReturnCorrectCoordinates()
        {
            var coordinatesValue = new CoordinatesValue(10.0, 20.0);
            Coordinates coordinates = coordinatesValue;
            Assert.Equal(10.0, coordinates.Latitude);
            Assert.Equal(20.0, coordinates.Longitude);
        }

        [Fact]
        public void ImplicitConversion_ToCoordinatesValue_ShouldReturnCorrectCoordinatesValue()
        {
            var coordinates = new Coordinates(10.0, 20.0);
            CoordinatesValue coordinatesValue = coordinates;
            Assert.Equal(10.0, coordinatesValue.Latitude);
            Assert.Equal(20.0, coordinatesValue.Longitude);
        }
    }
}
