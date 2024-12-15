using System.Threading.Tasks;
using Pmad.Cartography.Databases;

namespace Pmad.Cartography.Test.Databases
{
    public class WellKnownDatabasesTest
    {
        [Fact]
        public async Task AW3D30_ContainsLondon()
        {
            var database = WellKnownDatabases.GetAW3D30();

            await database.LoadIndexAsync();

            Assert.NotNull(await database.GetDataCellsAsync(new Coordinates(51, 0), new Coordinates(52, 1)));
        }

        [Fact]
        public async Task SRTM1_ContainsLondon()
        {
            var database = WellKnownDatabases.GetSRTM1();

            await database.LoadIndexAsync();

            Assert.NotNull(await database.GetDataCellsAsync(new Coordinates(51, 0), new Coordinates(52, 1)));
        }


        [Fact]
        public async Task SRTM15Plus_ContainsLondon()
        {
            var database = WellKnownDatabases.GetSRTM15Plus();

            await database.LoadIndexAsync();

            Assert.NotNull(await database.GetDataCellsAsync(new Coordinates(51, 0), new Coordinates(52, 1)));
        }
    }
}
