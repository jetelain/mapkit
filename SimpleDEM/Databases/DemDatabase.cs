using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleDEM.DataCells;

namespace SimpleDEM.Databases
{
    public class DemDatabase
    {
        private readonly IDemStorage storage;
        private readonly List<DemDatabaseEntry> entries = new List<DemDatabaseEntry>(); // TODO: Use a spacial index

        private readonly long maxBytes;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private long loadedBytes = 0;

        public DemDatabase(IDemStorage storage, long maxBytes = 8_000_000_000)
        {
            this.storage = storage;
            this.maxBytes = maxBytes;
        }

        public async Task LoadIndexAsync()
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                await LoadIndexInternal();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task LoadIndexInternal()
        {
            entries.Clear();
            loadedBytes = 0;
            entries.AddRange((await storage.ReadIndex()).Cells.Select(i => new DemDatabaseEntry(i.Path, i.Metadata)));
        }

        private async Task EnsureIndexIsLoadedAsync()
        {
            if (entries.Count == 0)
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    if (entries.Count == 0)
                    {
                        await LoadIndexInternal();
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }

        public async Task<List<IDemDataCell>> GetDataCellsAsync(Coordinates start, Coordinates end)
        {
            await EnsureIndexIsLoadedAsync();

            var datas = new List<IDemDataCell>();
            foreach(var cell in entries.Where(e => e.Overlaps(start, end)))
            {
                datas.Add(await GetDataAsync(cell));
            }
            return datas;
        }

        public double GetElevation(Coordinates coordinates, IInterpolation interpolation)
        {
            if (entries.Count == 0)
            {
                EnsureIndexIsLoadedAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            var cells = entries.Where(e => e.Contains(coordinates)).ToList();
            if (cells.Count == 0)
            {
                return double.NaN;
            }
            if (cells[0].Metadata.RasterType == DemRasterType.PixelIsPoint || cells.Count == 1)
            {
                return GetData(cells[0]).GetLocalElevation(coordinates, interpolation);
            }
            return GetElevationWithMultipleCells(coordinates, interpolation, cells.Select(GetData).ToList());
        }

        public async Task<double> GetElevationAsync(Coordinates coordinates, IInterpolation interpolation)
        {
            await EnsureIndexIsLoadedAsync();

            var cells = entries.Where(e => e.Contains(coordinates)).ToList();
            if (cells.Count == 0)
            {
                return double.NaN;
            }
            if (cells[0].Metadata.RasterType == DemRasterType.PixelIsPoint || cells.Count == 1)
            {
                return (await GetDataAsync(cells[0])).GetLocalElevation(coordinates, interpolation);
            }
            var datas = new List<IDemDataCell>();
            foreach (var cell in cells)
            {
                datas.Add(await GetDataAsync(cell));
            }
            return GetElevationWithMultipleCells(coordinates, interpolation, datas);
        }

        private static double GetElevationWithMultipleCells(Coordinates coordinates, IInterpolation interpolation, List<IDemDataCell> datas)
        {
            var local = datas.FirstOrDefault(d => d.IsLocal(coordinates));
            if (local != null)
            {
                return local.GetLocalElevation(coordinates, interpolation);
            }
            var points = datas.SelectMany(d => d.GetNearbyElevation(coordinates)).Distinct().ToList();
            return interpolation.Interpolate(coordinates, points);
        }

        private IDemDataCell GetData(DemDatabaseEntry demDatabaseEntry)
        {
            return GetDataAsync(demDatabaseEntry).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task<IDemDataCell> GetDataAsync(DemDatabaseEntry demDatabaseEntry)
        {
            var data = demDatabaseEntry.PickData();
            if (data == null)
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    data = await demDatabaseEntry.Load(storage);
                    loadedBytes += data.SizeInBytes;

                    while (loadedBytes >= maxBytes)
                    {
                        var older = entries.Where(d => d.Data != null).OrderBy(d => d.LastAccess).FirstOrDefault();
                        if (older == null)
                        {
                            throw new InvalidOperationException("Memory limit reached, but no data loaded.");
                        }
                        loadedBytes -= older.UnLoad();
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            return data;
        }

        public async Task ReleaseOlderDataAsync(TimeSpan timeSpan)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var old = DateTime.UtcNow.Add(timeSpan).Ticks;
                foreach (var entry in entries)
                {
                    if (entry.Data != null && entry.LastAccess < old)
                    {
                        loadedBytes -= entry.UnLoad();
                    }
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
