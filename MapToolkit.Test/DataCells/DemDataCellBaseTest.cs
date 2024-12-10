using System.IO;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.DataCells
{
    public class DemDataCellBaseTest
    {
        [Fact]
        public void PinToGridFloor()
        {
            var isPoint = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });
            Assert.Equal(0.5, isPoint.PixelSizeLat);
            Assert.Equal(0.5, isPoint.PixelSizeLon);
            Assert.Equal(new Coordinates(0, 0), isPoint.PinToGridFloor(new Coordinates(0.25, 0.25)));
            Assert.Equal(new Coordinates(0.5, 0.5), isPoint.PinToGridFloor(new Coordinates(0.75, 0.75)));
            Assert.Equal(new Coordinates(0.5, 0), isPoint.PinToGridFloor(new Coordinates(0.75, 0.25)));
            Assert.Equal(new Coordinates(1, 1), isPoint.PinToGridFloor(new Coordinates(1.25, 1.25)));
            Assert.Equal(new Coordinates(-1.5, -1.5), isPoint.PinToGridFloor(new Coordinates(-1.25, -1.25)));

            var isArea = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[2, 2] {
                { 6, 3 },
                { 5, 8 }
            });
            Assert.Equal(0.5, isArea.PixelSizeLat);
            Assert.Equal(0.5, isArea.PixelSizeLon);
            Assert.Equal(new Coordinates(0, 0), isArea.PinToGridFloor(new Coordinates(0.25, 0.25)));
            Assert.Equal(new Coordinates(0.5, 0.5), isArea.PinToGridFloor(new Coordinates(0.75, 0.75)));
            Assert.Equal(new Coordinates(0.5, 0), isArea.PinToGridFloor(new Coordinates(0.75, 0.25)));
            Assert.Equal(new Coordinates(1, 1), isArea.PinToGridFloor(new Coordinates(1.25, 1.25)));
            Assert.Equal(new Coordinates(-1.5, -1.5), isArea.PinToGridFloor(new Coordinates(-1.25, -1.25)));
        }
        [Fact]

        public void PinToGridCeiling()
        {
            var isPoint = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });
            Assert.Equal(0.5, isPoint.PixelSizeLat);
            Assert.Equal(0.5, isPoint.PixelSizeLon);
            Assert.Equal(new Coordinates(0.5, 0.5), isPoint.PinToGridCeiling(new Coordinates(0.25, 0.25)));
            Assert.Equal(new Coordinates(1, 1), isPoint.PinToGridCeiling(new Coordinates(0.75, 0.75)));
            Assert.Equal(new Coordinates(1, 0.5), isPoint.PinToGridCeiling(new Coordinates(0.75, 0.25)));
            Assert.Equal(new Coordinates(1.5, 1.5), isPoint.PinToGridCeiling(new Coordinates(1.25, 1.25)));
            Assert.Equal(new Coordinates(-1, -1), isPoint.PinToGridCeiling(new Coordinates(-1.25, -1.25)));

            var isArea = new DemDataCellPixelIsArea<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[2, 2] {
                { 6, 3 },
                { 5, 8 }
            });
            Assert.Equal(0.5, isArea.PixelSizeLat);
            Assert.Equal(0.5, isArea.PixelSizeLon);
            Assert.Equal(new Coordinates(0.5, 0.5), isArea.PinToGridCeiling(new Coordinates(0.25, 0.25)));
            Assert.Equal(new Coordinates(1, 1), isArea.PinToGridCeiling(new Coordinates(0.75, 0.75)));
            Assert.Equal(new Coordinates(1, 0.5), isArea.PinToGridCeiling(new Coordinates(0.75, 0.25)));
            Assert.Equal(new Coordinates(1.5, 1.5), isArea.PinToGridCeiling(new Coordinates(1.25, 1.25)));
            Assert.Equal(new Coordinates(-1, -1), isArea.PinToGridCeiling(new Coordinates(-1.25, -1.25)));
        }

        [Fact]
        public void SaveToStream()
        {
            var isPoint = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            using var memoryStream = new MemoryStream();
            isPoint.Save(memoryStream);
            var bytes = memoryStream.ToArray();

            Assert.Equal(78, bytes.Length);

            var read = (DemDataCellPixelIsPoint<short>)DemDataCell.Load(new MemoryStream(bytes));
            Assert.Equal(new Coordinates(0, 0), read.Start);
            Assert.Equal(new Coordinates(1, 1), read.End);
            Assert.Equal(DemRasterType.PixelIsPoint, read.RasterType);
            Assert.Equal(3, read.PointsLat);
            Assert.Equal(3, read.PointsLon);
            Assert.Equal(6, read.Data[0, 0]);
            Assert.Equal(3, read.Data[0, 1]);
            Assert.Equal(0, read.Data[0, 2]);
        }
        [Fact]
        public void FillVoidsFrom_FillsCorrectly()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, short.MaxValue, 4 },
                { 4, 7, short.MaxValue }
            };
            var isPoint = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);

            double GetElevation(Coordinates coordinates)
            {
                if (coordinates.Equals(new Coordinates(0.5, 0.5)))
                    return 8;
                if (coordinates.Equals(new Coordinates(1, 1)))
                    return 2;
                return double.NaN;
            }

            isPoint.FillVoidsFrom(GetElevation);

            Assert.Equal(8, isPoint.Data[1, 1]);
            Assert.Equal(2, isPoint.Data[2, 2]);
        }

        [Fact]
        public void FillVoidsFrom_NoVoids()
        {
            var data = new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            };
            var isPoint = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);

            double GetElevation(Coordinates coordinates)
            {
                return double.NaN;
            }

            isPoint.FillVoidsFrom(GetElevation);

            Assert.Equal(8, isPoint.Data[1, 1]);
            Assert.Equal(2, isPoint.Data[2, 2]);
        }

        [Fact]
        public void FillVoidsFrom_AllVoids()
        {
            var data = new short[3, 3] {
                { short.MaxValue, short.MaxValue, short.MaxValue },
                { short.MaxValue, short.MaxValue, short.MaxValue },
                { short.MaxValue, short.MaxValue, short.MaxValue }
            };
            var isPoint = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), data);

            double GetElevation(Coordinates coordinates)
            {
                return 1;
            }

            isPoint.FillVoidsFrom(GetElevation);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.Equal(1, isPoint.Data[i, j]);
                }
            }
        }

    }
}
