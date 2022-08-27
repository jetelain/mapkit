using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleDEM.DataCells;

namespace SimpleDEM.Test.DataCells
{
    /// <summary>
    /// Tests of <see cref="DemDataCellPixelIsPoint{T}"/>
    /// </summary>
    public class DemDataCellPixelIsPointTest
    {

        [Fact]
        public void GetRawElevation()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.1)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.2)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.3)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.4)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.5)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.6)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.4)));
            Assert.Equal(0, dataCell.GetRawElevation(new Coordinates(0, 0.8)));
            Assert.Equal(0, dataCell.GetRawElevation(new Coordinates(0, 0.9)));
            Assert.Equal(0, dataCell.GetRawElevation(new Coordinates(0, 1.0)));

            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.0, 0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.1, 0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.2, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.3, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.4, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.5, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.6, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.4, 0)));
            Assert.Equal(4, dataCell.GetRawElevation(new Coordinates(0.8, 0)));
            Assert.Equal(4, dataCell.GetRawElevation(new Coordinates(0.9, 0)));
            Assert.Equal(4, dataCell.GetRawElevation(new Coordinates(1.0, 0)));

            Assert.Equal(8, dataCell.GetRawElevation(new Coordinates(0.5, 0.5)));
            Assert.Equal(2, dataCell.GetRawElevation(new Coordinates(1.0, 1.0)));
        }

        [Fact]
        public void Crop()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            var subCell = dataCell.Crop(new Coordinates(0, 0), new Coordinates(0.5, 0.5)); 
            Assert.Equal(new Coordinates(0, 0), subCell.Start);
            Assert.Equal(new Coordinates(0.5, 0.5), subCell.End);
            Assert.Equal(0.5, subCell.SizeLon);
            Assert.Equal(0.5, subCell.SizeLat);
            Assert.Equal(2, subCell.PointsLat);
            Assert.Equal(2, subCell.PointsLon);
            Assert.Equal(6, subCell.Data[0, 0]);
            Assert.Equal(3, subCell.Data[0, 1]);
            Assert.Equal(5, subCell.Data[1, 0]);
            Assert.Equal(8, subCell.Data[1, 1]);

            subCell = dataCell.Crop(new Coordinates(0.5, 0.5), new Coordinates(1, 1));
            Assert.Equal(new Coordinates(0.5, 0.5), subCell.Start);
            Assert.Equal(new Coordinates(1, 1), subCell.End);
            Assert.Equal(0.5, subCell.SizeLon);
            Assert.Equal(0.5, subCell.SizeLat);
            Assert.Equal(2, subCell.PointsLat);
            Assert.Equal(2, subCell.PointsLon);
            Assert.Equal(8, subCell.Data[0, 0]);
            Assert.Equal(4, subCell.Data[0, 1]);
            Assert.Equal(7, subCell.Data[1, 0]);
            Assert.Equal(2, subCell.Data[1, 1]);
        }
    }
}
