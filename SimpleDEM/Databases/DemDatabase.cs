using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleDEM.DataCells;

namespace SimpleDEM.Databases
{
    public class DemDatabase
    {
        private readonly IDemStorage storage;
        private readonly List<DemDatabaseEntry> entries = new List<DemDatabaseEntry>();

        private readonly long maxBytes;
        private readonly object cacheLocker = new object();
        private long loadedBytes = 0;

        public DemDatabase(string basePath, long maxBytes = 8_000_000_000)
        {
            this.storage = new FileSystemStorage(basePath);
            this.maxBytes = maxBytes;
        }

        public void BuildIndex()
        {
            entries.Clear();
            entries.AddRange(storage.ReadIndex());
        }

        public double GetElevation(GeodeticCoordinates coordinates, IInterpolation interpolation)
        {
            var cells = entries.Where(e => e.Contains(coordinates)).ToList();
            if (cells.Count == 0)
            {
                return double.NaN;
            }

            if (cells[0].Metadata.RasterType == DemRasterType.PixelIsPoint || cells.Count == 1)
            {
                return GetData(cells[0]).GetLocalElevation(coordinates, interpolation);
            }

            var datas = cells.Select(GetData).ToList();
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
            var data = demDatabaseEntry.PickData();
            if (data == null)
            {
                lock (cacheLocker)
                {
                    data = demDatabaseEntry.Load(storage);
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
            }
            return data;
        }

        public void ReleaseOlderData(TimeSpan timeSpan)
        {
            lock (cacheLocker)
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
        }
    }
}
