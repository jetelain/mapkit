﻿using System.Collections.Generic;
using System.Linq;
using Pmad.Cartography.DataCells;
using Pmad.Geometry;

namespace Pmad.Cartography.Test.DataCells
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
            Assert.Equal(3, dataCell.GetRawElevation(new Coordinates(0, 0.7)));
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
            Assert.Equal(5, dataCell.GetRawElevation(new Coordinates(0.7, 0)));
            Assert.Equal(4, dataCell.GetRawElevation(new Coordinates(0.8, 0)));
            Assert.Equal(4, dataCell.GetRawElevation(new Coordinates(0.9, 0)));
            Assert.Equal(4, dataCell.GetRawElevation(new Coordinates(1.0, 0)));

            Assert.Equal(8, dataCell.GetRawElevation(new Coordinates(0.5, 0.5)));
            Assert.Equal(2, dataCell.GetRawElevation(new Coordinates(1.0, 1.0)));
        }

        [Fact]
        public void GetLocalElevation()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            Assert.Equal(5.50, dataCell.GetLocalElevation(new Coordinates(0.25, 0.25), DefaultInterpolation.Instance));
            Assert.Equal(6.00, dataCell.GetLocalElevation(new Coordinates(0.75, 0.25), DefaultInterpolation.Instance));
            Assert.Equal(3.75, dataCell.GetLocalElevation(new Coordinates(0.25, 0.75), DefaultInterpolation.Instance));
            Assert.Equal(5.25, dataCell.GetLocalElevation(new Coordinates(0.75, 0.75), DefaultInterpolation.Instance));
        }

        [Fact]
        public void GetLocalElevation_OutOfRange()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            Assert.True(double.IsNaN(dataCell.GetLocalElevation(new Coordinates(1.5, 1.5), DefaultInterpolation.Instance)));
            Assert.True(double.IsNaN(dataCell.GetLocalElevation(new Coordinates(-0.5, -0.5), DefaultInterpolation.Instance)));
            Assert.Equal(2, dataCell.GetLocalElevation(new Coordinates(1.25, 1.25), DefaultInterpolation.Instance));
            Assert.Equal(6, dataCell.GetLocalElevation(new Coordinates(-0.25, -0.25), DefaultInterpolation.Instance));
        }

        [Fact]
        public void GetPointsOnParallel()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0, 0), 6),
                new DemDataPoint(new Coordinates(0, 0.5), 3),
                new DemDataPoint(new Coordinates(0, 1), 0)
            }, dataCell.GetPointsOnParallel(0, 0, 3).ToList());

            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0, 0.5), 3)
            }, dataCell.GetPointsOnParallel(0, 1, 1).ToList());

            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(0.5, 0), 5),
                new DemDataPoint(new Coordinates(0.5, 0.5), 8),
                new DemDataPoint(new Coordinates(0.5, 1), 4)
            }, dataCell.GetPointsOnParallel(1, 0, 3).ToList());

            Assert.Equal(new List<DemDataPoint>() {
                new DemDataPoint(new Coordinates(1, 0), 4),
                new DemDataPoint(new Coordinates(1, 0.5), 7),
                new DemDataPoint(new Coordinates(1, 1), 2)
            }, dataCell.GetPointsOnParallel(2, 0, 3).ToList());
        }

        [Fact]
        public void CreateView()
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

        [Fact]
        public void Constructor_WithStartEndCoordinates()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            };
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);

            Assert.Equal(0, dataCell.Start.Latitude);
            Assert.Equal(0, dataCell.Start.Longitude);
            Assert.Equal(1, dataCell.End.Latitude);
            Assert.Equal(1, dataCell.End.Longitude);
            Assert.Equal(3, dataCell.PointsLat);
            Assert.Equal(3, dataCell.PointsLon);
            Assert.Equal(0.5, dataCell.PixelSizeLat);
            Assert.Equal(0.5, dataCell.PixelSizeLon);
        }


        [Fact]
        public void Constructor_WithStartAndPixelSize()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            };
            var pixelSize = new Vector2D(0.5, 0.5);
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), pixelSize, data);

            Assert.Equal(0, dataCell.Start.Latitude);
            Assert.Equal(0, dataCell.Start.Longitude);
            Assert.Equal(1, dataCell.End.Latitude);
            Assert.Equal(1, dataCell.End.Longitude);
            Assert.Equal(3, dataCell.PointsLat);
            Assert.Equal(3, dataCell.PointsLon);
            Assert.Equal(0.5, dataCell.PixelSizeLat);
            Assert.Equal(0.5, dataCell.PixelSizeLon);
        }

        [Fact]
        public void Constructor_WithStartAndPixelSize_NonSquare()
        {
            var data = new short[, ] {
                { 6, 3, 0 },
                { 4, 7, 2 }
            };
            var pixelSize = new Vector2D(0.5, 0.5);
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), pixelSize, data);

            Assert.Equal(0, dataCell.Start.Latitude);
            Assert.Equal(0, dataCell.Start.Longitude);
            Assert.Equal(0.5, dataCell.End.Latitude);
            Assert.Equal(1, dataCell.End.Longitude);
            Assert.Equal(2, dataCell.PointsLat);
            Assert.Equal(3, dataCell.PointsLon);
            Assert.Equal(0.5, dataCell.PixelSizeLat);
            Assert.Equal(0.5, dataCell.PixelSizeLon);
        }

        [Fact]
        public void ConvertTo()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            };
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);
            var convertedDataCell = dataCell.ConvertTo<int>();

            Assert.Equal(dataCell.Start, convertedDataCell.Start);
            Assert.Equal(dataCell.End, convertedDataCell.End);
            Assert.Equal(dataCell.PointsLat, convertedDataCell.PointsLat);
            Assert.Equal(dataCell.PointsLon, convertedDataCell.PointsLon);
            Assert.Equal(dataCell.PixelSizeLat, convertedDataCell.PixelSizeLat);
            Assert.Equal(dataCell.PixelSizeLon, convertedDataCell.PixelSizeLon);
        }

        [Fact]
        public void AsPixelIsArea()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            };
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);
            var areaDataCell = dataCell.AsPixelIsArea();

            Assert.Equal(new Coordinates(-0.25, -0.25), areaDataCell.Start);
            Assert.Equal(new Coordinates(1.25, 1.25), areaDataCell.End);
            Assert.Equal(dataCell.PointsLat, areaDataCell.PointsLat);
            Assert.Equal(dataCell.PointsLon, areaDataCell.PointsLon);
        }

        [Fact]
        public void AsPixelIsPoint()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            };
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);

            Assert.Same(dataCell, dataCell.AsPixelIsPoint());
        }

        [Fact]
        public void IsLocal()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            };
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);

            Assert.True(dataCell.IsLocal(new Coordinates(0.5, 0.5)));
            Assert.False(dataCell.IsLocal(new Coordinates(1.5, 1.5)));
        }

        [Fact]
        public void Downsample()
        {
            var data = new short[5, 5] {
                { 1, 2, 3, 4, 5 },
                { 6, 7, 8, 9, 10 },
                { 11, 12, 13, 14, 15 },
                { 16, 17, 18, 19, 20 },
                { 21, 22, 23, 24, 25 }
            };
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);
            var downsampledDataCell = dataCell.Downsample(2, null, null, null);

            Assert.Equal(3, downsampledDataCell.PointsLat);
            Assert.Equal(3, downsampledDataCell.PointsLon);

            Assert.Equal(4, downsampledDataCell.Data[0, 0]);
            Assert.Equal(6, downsampledDataCell.Data[0, 1]);
            Assert.Equal(5, downsampledDataCell.Data[0, 2]);

            Assert.Equal(14, downsampledDataCell.Data[1, 0]);
            Assert.Equal(16, downsampledDataCell.Data[1, 1]);
            Assert.Equal(20, downsampledDataCell.Data[1, 2]);

            Assert.Equal(12, downsampledDataCell.Data[2, 0]);
            Assert.Equal(15, downsampledDataCell.Data[2, 1]);
            Assert.Equal(25, downsampledDataCell.Data[2, 2]);
        }

        [Fact]
        public void GetNearbyElevation()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            // x
            //   +---+---+
            //   |   |   |
            //   +---+---+ 
            //   |   |   |
            //   +---+---+
            var dataPoints = dataCell.GetNearbyElevation(new Coordinates(-0.25, -0.25));
            Assert.Equal(new DemDataPoint(new Coordinates(0, 0), 6), Assert.Single(dataPoints));

            //             x
            //   +---+---+
            //   |   |   |
            //   +---+---+ 
            //   |   |   |
            //   +---+---+
            dataPoints = dataCell.GetNearbyElevation(new Coordinates(-0.25, 1.25));
            Assert.Equal(new DemDataPoint(new Coordinates(0, 1), 0), Assert.Single(dataPoints));

            //   +---+---+
            //   |   |   |
            //   +---+---+ 
            //   |   |   |
            //   +---+---+
            // x
            dataPoints = dataCell.GetNearbyElevation(new Coordinates(1.25, -0.25));
            Assert.Equal(new DemDataPoint(new Coordinates(1, 0), 4), Assert.Single(dataPoints));

            //   +---+---+
            //   |   |   |
            //   +---+---+ 
            //   |   |   |
            //   +---+---+
            //            x
            dataPoints = dataCell.GetNearbyElevation(new Coordinates(1.25, 1.25));
            Assert.Equal(new DemDataPoint(new Coordinates(1, 1), 2), Assert.Single(dataPoints));

            //   +---+---+
            //   |   |   |
            //   +---+---+ 
            //   |   | x |
            //   +---+---+
            dataPoints = dataCell.GetNearbyElevation(new Coordinates(0.75, 0.75));
            Assert.Equal(new List<DemDataPoint>()
            {
                new DemDataPoint(new Coordinates(0.5, 0.5), 8),
                new DemDataPoint(new Coordinates(1, 0.5), 7),
                new DemDataPoint(new Coordinates(0.5, 1), 4),
                new DemDataPoint(new Coordinates(1, 1), 2)
            }, dataPoints.ToList());

            //   +---+---+
            //   |   |   |
            //   +---+---+ 
            //   |   |   |
            //   +---+---+
            //         x

            dataPoints = dataCell.GetNearbyElevation(new Coordinates(1.25, 0.75));
            Assert.Equal(new List<DemDataPoint>()
            {
                new DemDataPoint(new Coordinates(1, 0.5), 7),
                new DemDataPoint(new Coordinates(1, 1), 2)
            }, dataPoints.ToList());

            //   +---+---+
            //   |   |   |
            //   +---+---+ 
            //   |   |   | x
            //   +---+---+

            dataPoints = dataCell.GetNearbyElevation(new Coordinates(0.75, 1.25));
            Assert.Equal(new List<DemDataPoint>()
            {
                new DemDataPoint(new Coordinates(0.5, 1), 4),
                new DemDataPoint(new Coordinates(1, 1), 2)
            }, dataPoints.ToList());

        }

        [Fact]
        public void GetNearbyElevation_OutOfRange()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(-0.5, -0.5)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(-0.75, -0.75)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(1.5, 1.5)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(1.75, 1.75)));

            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(0.5, -0.5)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(0.5, -0.75)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(0.5, 1.5)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(0.5, 1.75)));

            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(-0.5, 0.5)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(-0.75, 0.5)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(1.5, 0.5)));
            Assert.Empty(dataCell.GetNearbyElevation(new Coordinates(1.75, 0.5)));
        }
    }
}
