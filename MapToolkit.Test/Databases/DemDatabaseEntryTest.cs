using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;

namespace Pmad.Cartography.Test.Databases
{
    public class DemDatabaseEntryTest
    {
        [Fact]
        public void DemDatabaseEntry_GetCoverageSurface()
        {
            var entry = new DemDatabaseEntry("test", new DemDataCellMetadata(DemRasterType.PixelIsPoint, new Coordinates(1, 1), new Coordinates(2, 2), 100, 100));
            Assert.Equal(0, entry.GetCoverageSurface(new Coordinates(0, 0), new Coordinates(1, 1)));
            Assert.Equal(1, entry.GetCoverageSurface(new Coordinates(1, 1), new Coordinates(2, 2)));
            Assert.Equal(0, entry.GetCoverageSurface(new Coordinates(2, 2), new Coordinates(3, 3)));
            Assert.Equal(0.5, entry.GetCoverageSurface(new Coordinates(1.5, 1), new Coordinates(2.5, 2)));
            Assert.Equal(0.25, entry.GetCoverageSurface(new Coordinates(1.5, 1.5), new Coordinates(2.5, 2.5)));
        }
    }
}
