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
    }
}
