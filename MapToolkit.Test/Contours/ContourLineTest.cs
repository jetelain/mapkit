using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapToolkit.Contours;

namespace MapToolkit.Test.Contours
{
    public class ContourLineTest
    {
        [Fact]
        public void IsCounterClockWise()
        {
            /*
             LAT
              ^
              |
              +       A
              |
              +   C       B
              |
              +---+---+---+--> LON
            */

            var cl = new ContourLine(new[] {
                new CoordinatesValue(2,2), // A
                new CoordinatesValue(1,3), // B
                new CoordinatesValue(1,1), // C
                new CoordinatesValue(2,2)  // A
            }, 100);

            Assert.False(cl.IsCounterClockWise);
            Assert.True(cl.IsClosed);

            /*
             LAT
              ^
              |
              +       A
              |
              +   B       C
              |
              +---+---+---+--> LON
            */

            cl = new ContourLine(new[] {
                new CoordinatesValue(2,2), // A
                new CoordinatesValue(1,1), // B
                new CoordinatesValue(1,3), // C
                new CoordinatesValue(2,2)  // A
            }, 100);

            Assert.True(cl.IsCounterClockWise);
            Assert.True(cl.IsClosed);
        }
    }
}
