using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pmad.Cartography.Test
{
    public class DefaultInterpolationTest
    {

        [Fact]
        public void InterpolatePointsList()
        {
            Assert.Equal(10,
                DefaultInterpolation.Instance.Interpolate(new Coordinates(1, 1), new List<DemDataPoint>() {
                    new DemDataPoint(new Coordinates(2,2), 10)
                }));

            Assert.Equal(10,
                DefaultInterpolation.Instance.Interpolate(new Coordinates(1, 1), new List<DemDataPoint>() {
                    new DemDataPoint(new Coordinates(2,2), 15),
                    new DemDataPoint(new Coordinates(0,0), 5),
                })); 
            
            Assert.Equal(15,
                DefaultInterpolation.Instance.Interpolate(new Coordinates(2, 2), new List<DemDataPoint>() {
                    new DemDataPoint(new Coordinates(2,2), 15),
                    new DemDataPoint(new Coordinates(0,0), 5),
                }));

            Assert.True(double.IsNaN(DefaultInterpolation.Instance.Interpolate(new Coordinates(1, 1), new List<DemDataPoint>())));
        }
    }
}
