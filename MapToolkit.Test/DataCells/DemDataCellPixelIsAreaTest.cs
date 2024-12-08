using System.Collections.Generic;
using System.Linq;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.DataCells
{
    /// <summary>
    /// Tests of <see cref="DemDataCellPixelIsArea{T}"/>
    /// </summary>
    public class DemDataCellPixelIsAreaTest
    {

        [Fact]
        public void GetRawElevation()
        {
            var dataCell = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[2, 2] {
                { 6, 3 },
                { 5, 8 },
            });

            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.1)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.2)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.3)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0, 0.4)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.5)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.6)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.7)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.8)));
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.9)));

            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.0, 0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.1, 0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.2, 0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.3, 0)));
            Assert.Equal(6, dataCell.GetRawElevation(new Coordinates(0.4, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.5, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.6, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.7, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.8, 0)));
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.9, 0)));

            Assert.Equal(8, dataCell.GetRawElevation(new Coordinates(0.5, 0.5)));
            Assert.Equal(8, dataCell.GetRawElevation(new Coordinates(0.9, 0.9)));
        }

        [Fact]
        public void GetLocalElevation()
        {
            var dataCell = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[2, 2] {
                { 6, 3 },
                { 5, 8 },
            });

            Assert.Equal(5.5, dataCell.GetLocalElevation(new Coordinates(0.5, 0.5), DefaultInterpolation.Instance));
        }

        [Fact]
        public void GetPointsOnParallel()
        {
            var dataCell = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[2, 2] {
                { 6, 3 },
                { 5, 8 },
            });

            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.25, 0.25), 6),
                new DemDataPoint(new Coordinates(0.25, 0.75), 3),
            }, dataCell.GetPointsOnParallel(0, 0, 2).ToList());

            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.25, 0.75), 3),
            }, dataCell.GetPointsOnParallel(0, 1, 1).ToList());

            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.75, 0.25), 5),
                new DemDataPoint(new Coordinates(0.75, 0.75), 8),
            }, dataCell.GetPointsOnParallel(1, 0, 2).ToList());
        }

        [Fact]
        public void Downsample()
        {
            var dataCell = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(2, 2), new short[4, 4] {
                    { 1, 2, 3, 4 },
                    { 5, 6, 7, 8 },
                    { 9, 10, 11, 12 },
                    { 13, 14, 15, 16 },
                });

            var downsampled = dataCell.Downsample(2);

            Assert.Equal(2, downsampled.Data.GetLength(0));
            Assert.Equal(2, downsampled.Data.GetLength(1));

            Assert.Equal(3, downsampled.Data[0, 0]);
            Assert.Equal(5, downsampled.Data[0, 1]);

            Assert.Equal(11, downsampled.Data[1, 0]);
            Assert.Equal(13, downsampled.Data[1, 1]);
        }

        [Fact]
        public void Crop()
        {
            var dataCell = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(2, 2), new short[4, 4] {
                    { 1, 2, 3, 4 },
                    { 5, 6, 7, 8 },
                    { 9, 10, 11, 12 },
                    { 13, 14, 15, 16 },
                });

            var subCell = dataCell.Crop(new Coordinates(0, 0), new Coordinates(1, 1));
            Assert.Equal(new Coordinates(0, 0), subCell.Start);
            Assert.Equal(new Coordinates(1, 1), subCell.End);

            Assert.Equal(2, subCell.Data.GetLength(0));
            Assert.Equal(2, subCell.Data.GetLength(1));

            Assert.Equal(1, subCell.Data[0, 0]);
            Assert.Equal(2, subCell.Data[0, 1]);

            Assert.Equal(5, subCell.Data[1, 0]);
            Assert.Equal(6, subCell.Data[1, 1]);
        }
    }
}
