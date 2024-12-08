using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pmad.Cartography.Databases;

namespace Pmad.Cartography.Test.Databases
{
    public class WellKnownDatabasesTest
    {
        private const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0) Gecko/20100101 Firefox/93.0";

        [Fact]
        public async Task AW3D30_ContainsLondon()
        {
            var baseAddress = "https://cdn.dem.pmad.net/AW3D30/";

            var storage = await CreateDatabaseAndLoadIndex(baseAddress);

            Assert.NotNull(await storage.GetDataCellsAsync(new Coordinates(51, 0), new Coordinates(52, 1)));
        }

        [Fact]
        public async Task SRTM1_ContainsLondon()
        {
            var baseAddress = "https://cdn.dem.pmad.net/SRTM1/";

            var storage = await CreateDatabaseAndLoadIndex(baseAddress);

            Assert.NotNull(await storage.GetDataCellsAsync(new Coordinates(51, 0), new Coordinates(52, 1)));
        }


        [Fact]
        public async Task SRTM15Plus_ContainsLondon()
        {
            var baseAddress = "https://cdn.dem.pmad.net/SRTM15Plus/";

            var storage = await CreateDatabaseAndLoadIndex(baseAddress);

            Assert.NotNull(await storage.GetDataCellsAsync(new Coordinates(51, 0), new Coordinates(52, 1)));
        }

        private static async Task<DemDatabase> CreateDatabaseAndLoadIndex(string baseAddress)
        {
            var localCache = Path.Combine(Path.GetTempPath(), "dem_test_wellknown");
            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            var storage = new DemDatabase(new DemHttpStorage(localCache, httpClient));
            await storage.LoadIndexAsync();
            return storage;
        }
    }
}
