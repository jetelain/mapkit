using MapToolkit.DataCells;
using SixLabors.ImageSharp.PixelFormats;

namespace MapToolkit.Test.DataCells
{
    public class DemDataViewExtensionsTest
    {
        [Fact]
        public void ToImage()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(0, 0), new Coordinates(1, 1), new short[3, 3] {
                { 6, 3, 0 },
                { 5, 8, 4 },
                { 4, 7, 2 }
            });

            var image = dataCell.ToImage(e => new L16((ushort)e));

            Assert.Equal(4, image[0, 0].PackedValue);
            Assert.Equal(5, image[0, 1].PackedValue);
            Assert.Equal(6, image[0, 2].PackedValue);

            Assert.Equal(7, image[1, 0].PackedValue);
            Assert.Equal(8, image[1, 1].PackedValue);
            Assert.Equal(3, image[1, 2].PackedValue);

            Assert.Equal(2, image[2, 0].PackedValue);
            Assert.Equal(4, image[2, 1].PackedValue);
            Assert.Equal(0, image[2, 2].PackedValue);
        }

        [Fact]
        public void ToImage_ReversedY()
        {
            var dataCell = new DemDataCellPixelIsPoint<short>(new Coordinates(1, 0), new Coordinates(0, 1), new short[3, 3] {
                { 4, 7, 2 },
                { 5, 8, 4 },
                { 6, 3, 0 },
            });

            var image = dataCell.ToImage(e => new L16((ushort)e));

            Assert.Equal(4, image[0, 0].PackedValue);
            Assert.Equal(5, image[0, 1].PackedValue);
            Assert.Equal(6, image[0, 2].PackedValue);

            Assert.Equal(7, image[1, 0].PackedValue);
            Assert.Equal(8, image[1, 1].PackedValue);
            Assert.Equal(3, image[1, 2].PackedValue);

            Assert.Equal(2, image[2, 0].PackedValue);
            Assert.Equal(4, image[2, 1].PackedValue);
            Assert.Equal(0, image[2, 2].PackedValue);
        }
    }
}
