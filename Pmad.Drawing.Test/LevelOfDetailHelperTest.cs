using Pmad.Geometry;

namespace Pmad.Drawing.Test
{
    public class LevelOfDetailHelperTest
    {

        [Fact]
        public void LevelOfDetailHelper_SimplifyAngles()
        {
            var result = LevelOfDetailHelper.SimplifyAngles(new List<Vector2D>() { 
                new Vector2D(0,0),
                new Vector2D(0.25,10),
                new Vector2D(0,20)
            });

            Assert.Equal(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(0,20)}, 
                result);

            result = LevelOfDetailHelper.SimplifyAngles(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(0.25,10),
                new Vector2D(20,20)
            });

            Assert.Equal(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(0.25,10),
                new Vector2D(20,20)},
                result);
        }

        [Fact]
        public void LevelOfDetailHelper_SimplifyAnglesAndDistances()
        {
            var result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(0.25,10),
                new Vector2D(0,20)
            });

            Assert.Equal(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(0,20)},
                result);

            result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(0.25,10),
                new Vector2D(20,20)
            });

            Assert.Equal(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(0.25,10),
                new Vector2D(20,20)},
                result);

            result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(1,0),
                new Vector2D(2,0),
                new Vector2D(3,0),
                new Vector2D(4,0),
                new Vector2D(4,4),
                new Vector2D(5,4),
                new Vector2D(5,0),
                new Vector2D(10,0)
            }, 1.5);

            Assert.Equal(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(10,0) },
                result);

            result = LevelOfDetailHelper.SimplifyAnglesAndDistances(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(1,0),
                new Vector2D(2,0),
                new Vector2D(3,0),
                new Vector2D(4,0),
                new Vector2D(4,4),
                new Vector2D(5,4),
                new Vector2D(5,0),
                new Vector2D(10,0)
            }, 0.5);

            Assert.Equal(new List<Vector2D>() {
                new Vector2D(0,0),
                new Vector2D(4,0),
                new Vector2D(4,4),
                new Vector2D(5,4),
                new Vector2D(5,0),
                new Vector2D(10,0) },
                result);

        }
        // 


    }
}
