using System.Collections.Generic;
using System.Linq;
using MapToolkit.DataCells;

namespace MapToolkit.Test.DataCells
{
    public class DemDataViewTest
    {
        [Fact]
        public void PixelIsPoint_GetPointsOnParallel()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            var view = dataCell.CreateView(new Coordinates(0, 0), new Coordinates(0.5, 0.5));
            Assert.Equal(2, view.PointsLat);
            Assert.Equal(2, view.PointsLon);
            Assert.Equal(new Coordinates(0, 0), view.Start);
            Assert.Equal(new Coordinates(0.5, 0.5), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0, 0), 6),
                new DemDataPoint(new Coordinates(0, 0.5), 3)
            }, view.GetPointsOnParallel(0, 0, 2).ToList());

            view = dataCell.CreateView(new Coordinates(0.5, 0), new Coordinates(1, 0.5));
            Assert.Equal(2, view.PointsLat);
            Assert.Equal(2, view.PointsLon);
            Assert.Equal(new Coordinates(0.5, 0), view.Start);
            Assert.Equal(new Coordinates(1, 0.5), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.5, 0), 5),
                new DemDataPoint(new Coordinates(0.5, 0.5), 8)
            }, view.GetPointsOnParallel(0, 0, 2).ToList());

            view = dataCell.CreateView(new Coordinates(0.5, 0.5), new Coordinates(1, 1));
            Assert.Equal(2, view.PointsLat);
            Assert.Equal(2, view.PointsLon);
            Assert.Equal(new Coordinates(0.5, 0.5), view.Start);
            Assert.Equal(new Coordinates(1, 1), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.5, 0.5), 8),
                new DemDataPoint(new Coordinates(0.5, 1), 4)
            }, view.GetPointsOnParallel(0, 0, 2).ToList());

            view = dataCell.CreateView(new Coordinates(0.5, 0.5), new Coordinates(1.5, 1.5));
            Assert.Equal(3, view.PointsLat);
            Assert.Equal(3, view.PointsLon);
            Assert.Equal(new Coordinates(0.5, 0.5), view.Start);
            Assert.Equal(new Coordinates(1.5, 1.5), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.5, 0.5), 8),
                new DemDataPoint(new Coordinates(0.5, 1), 4),
                new DemDataPoint(new Coordinates(0.5, 1.5), 0)
            }, view.GetPointsOnParallel(0, 0, 3).ToList());
        }

        [Fact]
        public void PixelIsArea_GetPointsOnParallel()
        {
            var dataCell = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[4, 4] {
                { 6, 3, 1, 2 },
                { 5, 8, 4, 5 },
                { 4, 7, 2, 7 },
                { 3, 1, 8, 9 }
            });

            var view = dataCell.CreateView(new Coordinates(0, 0), new Coordinates(0.5, 0.5));
            Assert.Equal(2, view.PointsLat);
            Assert.Equal(2, view.PointsLon);
            Assert.Equal(new Coordinates(0, 0), view.Start);
            Assert.Equal(new Coordinates(0.5, 0.5), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.125, 0.125), 6),
                new DemDataPoint(new Coordinates(0.125, 0.375), 3)
            }, view.GetPointsOnParallel(0, 0, 2).ToList());

            view = dataCell.CreateView(new Coordinates(0.5, 0), new Coordinates(1, 0.5));
            Assert.Equal(2, view.PointsLat);
            Assert.Equal(2, view.PointsLon);
            Assert.Equal(new Coordinates(0.5, 0), view.Start);
            Assert.Equal(new Coordinates(1, 0.5), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.625, 0.125), 4),
                new DemDataPoint(new Coordinates(0.625, 0.375), 7)
            }, view.GetPointsOnParallel(0, 0, 2).ToList());

            view = dataCell.CreateView(new Coordinates(0.5, 0.5), new Coordinates(1, 1));
            Assert.Equal(2, view.PointsLat);
            Assert.Equal(2, view.PointsLon);
            Assert.Equal(new Coordinates(0.5, 0.5), view.Start);
            Assert.Equal(new Coordinates(1, 1), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.625, 0.625), 2),
                new DemDataPoint(new Coordinates(0.625, 0.875), 7)
            }, view.GetPointsOnParallel(0, 0, 2).ToList());

            view = dataCell.CreateView(new Coordinates(0.5, 0.5), new Coordinates(1.5, 1.5));
            Assert.Equal(4, view.PointsLat);
            Assert.Equal(4, view.PointsLon);
            Assert.Equal(new Coordinates(0.5, 0.5), view.Start);
            Assert.Equal(new Coordinates(1.5, 1.5), view.End);
            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.625, 0.625), 2),
                new DemDataPoint(new Coordinates(0.625, 0.875), 7),
                new DemDataPoint(new Coordinates(0.625, 1.125), 0),
                new DemDataPoint(new Coordinates(0.625, 1.375), 0)
            }, view.GetPointsOnParallel(0, 0, 4).ToList());

        }

        [Fact]
        public void PixelIsPoint_ToDataCell()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            var subCell = dataCell.CreateView(new Coordinates(0, 0), new Coordinates(0.5, 0.5)).ToDataCell();
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

            subCell = dataCell.CreateView(new Coordinates(0.5, 0.5), new Coordinates(1, 1)).ToDataCell();
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
