using System.Collections.Generic;
using Pmad.Cartography.Drawing;

namespace Pmad.Cartography.Drawing.Test
{
    public class LevelOfDetailHelperTest
    {

        [Fact]
        public void LevelOfDetailHelper_SimplifyAngles()
        {
            var result = LevelOfDetailHelper.SimplifyAngles(new List<Vector>() { 
                new Vector(0,0),
                new Vector(0.25,10),
                new Vector(0,20)
            });

            Assert.Equal(new List<Vector>() {
                new Vector(0,0),
                new Vector(0,20)}, 
                result);

            result = LevelOfDetailHelper.SimplifyAngles(new List<Vector>() {
                new Vector(0,0),
                new Vector(0.25,10),
                new Vector(20,20)
            });

            Assert.Equal(new List<Vector>() {
                new Vector(0,0),
                new Vector(0.25,10),
                new Vector(20,20)},
                result);
        }

        [Fact]
        public void LevelOfDetailHelper_SimplifyAnglesAndDistances()
        {
            var result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector>() {
                new Vector(0,0),
                new Vector(0.25,10),
                new Vector(0,20)
            });

            Assert.Equal(new List<Vector>() {
                new Vector(0,0),
                new Vector(0,20)},
                result);

            result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector>() {
                new Vector(0,0),
                new Vector(0.25,10),
                new Vector(20,20)
            });

            Assert.Equal(new List<Vector>() {
                new Vector(0,0),
                new Vector(0.25,10),
                new Vector(20,20)},
                result);

            result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector>() {
                new Vector(0,0),
                new Vector(1,0),
                new Vector(2,0),
                new Vector(3,0),
                new Vector(4,0),
                new Vector(4,4),
                new Vector(5,4),
                new Vector(5,0),
                new Vector(10,0)
            }, 1.5);

            Assert.Equal(new List<Vector>() {
                new Vector(0,0),
                new Vector(10,0) },
                result);

            result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector>() {
                new Vector(0,0),
                new Vector(1,0),
                new Vector(2,0),
                new Vector(3,0),
                new Vector(4,0),
                new Vector(4,4),
                new Vector(5,4),
                new Vector(5,0),
                new Vector(10,0)
            }, 0.5);

            Assert.Equal(new List<Vector>() {
                new Vector(0,0),
                new Vector(4,0),
                new Vector(4,4),
                new Vector(5,4),
                new Vector(5,0),
                new Vector(10,0) },
                result);

        }
        // 


    }
}
