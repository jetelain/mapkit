using System;
using System.Collections.Generic;
using Xunit;
using Pmad.Cartography.Contours;

namespace Pmad.Cartography.Test.Contours
{
    public class ContourLevelGeneratorTest
    {
        [Fact]
        public void Levels_ShouldReturnCorrectLevels()
        {
            var generator = new ContourLevelGenerator(0, 10);
            var levels = generator.Levels(5, 35);
            var expected = new List<double> { 10, 20, 30, 40 };
            Assert.Equal(expected, levels);
        }

        [Fact]
        public void Levels_ShouldReturnEmpty_WhenMinEqualsMax()
        {
            var generator = new ContourLevelGenerator(0, 10);
            var levels = generator.Levels(10, 10);
            Assert.Empty(levels);
        }

        [Fact]
        public void Levels_ShouldRespectStrictMin()
        {
            var generator = new ContourLevelGenerator(20, 10);
            var levels = generator.Levels(5, 35);
            var expected = new List<double> { 20, 30, 40 };
            Assert.Equal(expected, levels);
        }

        [Fact]
        public void Levels_ShouldHandleNegativeValues()
        {
            var generator = new ContourLevelGenerator(-30, 10);
            var levels = generator.Levels(-25, 5);
            var expected = new List<double> { -20, -10, 0, 10 };
            Assert.Equal(expected, levels);
        }
    }
}
