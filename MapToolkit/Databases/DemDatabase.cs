using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pmad.Cartography.DataCells;
using Microsoft.Extensions.Caching.Memory;

namespace Pmad.Cartography.Databases
{
    public class DemDatabase
    {
        private readonly IMemoryCache cache;
        private readonly IDemStorage storage;
        private readonly List<DemDatabaseEntry> entries = new List<DemDatabaseEntry>();
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public DemDatabase(IDemStorage storage, IMemoryCache cache)
        {
            this.storage = storage;
            this.cache = cache;
            
        }

        public DemDatabase(IDemStorage storage)
            : this(storage, new MemoryCache(new MemoryCacheOptions() { SizeLimit = 1_000_000_000, CompactionPercentage = 0.5 }))
        {

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
            foreach(var entry in entries)
            {
                entry.UnLoad(cache);
            }
            entries.Clear();
            entries.AddRange((await storage.ReadIndex().ConfigureAwait(false)).Cells.Select(i => new DemDatabaseEntry(i.Path, i.Metadata)));
        }

        private async Task EnsureIndexIsLoadedAsync()
        {
            if (entries.Count == 0)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (entries.Count == 0)
                    {
                        await LoadIndexInternal().ConfigureAwait(false);
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
            await EnsureIndexIsLoadedAsync().ConfigureAwait(false);

            var datas = new List<IDemDataCell>();
            foreach(var cell in entries.Where(e => e.Overlaps(start, end)))
            {
                datas.Add(await GetDataAsync(cell).ConfigureAwait(false));
            }
            return datas;
        }

        public async Task<DemDataView<TPixel>> CreateView<TPixel>(Coordinates start, Coordinates end)
            where TPixel : unmanaged
        {
            var cells = await GetDataCellsAsync(start, end).ConfigureAwait(false);
            var converted = cells.Select(c => c.To<TPixel>()).ToList();
            if (converted.Count == 0)
            {
                throw new ArgumentException($"Requested area {start}->{end} is not convered by database.");
            }
            return new DemDataView<TPixel>(converted, start, end);
        }

        public async Task<bool> HasData(Coordinates start, Coordinates end)
        {
            await EnsureIndexIsLoadedAsync().ConfigureAwait(false);
            return entries.Any(e => e.Overlaps(start, end));
        }

        public async Task<bool> HasFullData(Coordinates start, Coordinates end)
        {
            await EnsureIndexIsLoadedAsync().ConfigureAwait(false);
            var surface = (end - start).Area();
            var coverage = 0d;
            foreach(var entry in entries.Where(e => e.Overlaps(start, end)))
            {
                coverage += entry.GetCoverageSurface(start, end);
            }
            return (surface - coverage) <= 0.000_000_000_1;
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
            var data = demDatabaseEntry.PickData(cache);
            if (data == null)
            {
                await semaphoreSlim.WaitAsync(); // loads only a cell a time
                try
                {
                    data = await demDatabaseEntry.Load(storage, cache).ConfigureAwait(false);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            return data;
        }
    }
}
