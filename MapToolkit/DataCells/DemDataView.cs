using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.DataCells
{
    public class DemDataView<TPixel> : IDemDataView
        where TPixel : unmanaged
    {
        internal class DemDataViewCell
        {
            internal readonly DemDataCellBase<TPixel> cell;
            internal readonly int lat;
            internal readonly int lon;
            internal readonly int endLat;
            internal readonly int endLon;

            internal DemDataViewCell(DemDataCellBase<TPixel> cell, CellCoordinates start)
            {
                this.cell = cell;
                this.lat = start.Latitude;
                this.lon = start.Longitude;
                this.endLat = start.Latitude + cell.PointsLat;
                this.endLon = start.Longitude + cell.PointsLon;
            }
        }

        private readonly List<DemDataViewCell> cellsData = new List<DemDataViewCell>();
        private readonly double defaultValue = 0;

        public DemDataView(IEnumerable<DemDataCellBase<TPixel>> cells, Coordinates wantedStart, Coordinates wantedEnd)
        {
            var pixelSizeLat = Unique(cells.Select(c => c.PixelSizeLat));
            var pixelSizeLon = Unique(cells.Select(c => c.PixelSizeLon));
            var rasterType = cells.Select(c => c.RasterType).First();
            var pinnedStart = Unique(cells.Select(c => c.PinToGridCeiling(wantedStart)));
            var pinnedEnd = Unique(cells.Select(c => c.PinToGridFloor(wantedEnd)));

            Mapping = RasterMapping.Create(rasterType, pinnedStart, pinnedEnd, pixelSizeLat, pixelSizeLon);

            foreach(var cell in cells)
            {
                cellsData.Add(new DemDataViewCell(cell, Mapping.CoordinatesToIndexClosest(cell.Start)));
            }
        }

        public RasterMapping Mapping { get; }

        public double PixelSizeLat => Mapping.PixelSizeLat;

        public double PixelSizeLon => Mapping.PixelSizeLon;

        public DemRasterType RasterType => Mapping.RasterType;

        public Coordinates Start => Mapping.Start;

        public Coordinates End => Mapping.End;

        public double SizeLat => Mapping.SizeLat;

        public double SizeLon => Mapping.SizeLon;

        public int PointsLat => Mapping.PointsLat;

        public int PointsLon => Mapping.PointsLon;

        private static T Unique<T>(IEnumerable<T> enumerable) where  T: IEquatable<T>
        {
            var first = enumerable.First();
            if (enumerable.Skip(1).Any(v => !v.Equals(first)))
            {
                throw new ArgumentException("All cells must have the same resolution and grid cap.");
            }
            return first;
        }

        public IEnumerable<DemDataPoint> GetPointsOnParallel(int lat, int startLon, int count)
        {
            var lon = startLon;
            var endLon = startLon + count;
            foreach (var cell in cellsData.Where(c => c.lat <= lat && lat <= c.endLat && c.endLon >= startLon && c.lon <= endLon).OrderBy(c => c.lon))
            {
                int deltaLon = cell.lon - lon;
                if (deltaLon < 0)
                {
                    // fill gap up to cell
                    while(lon < cell.lon)
                    {
                        yield return new DemDataPoint(Mapping.IndexToCoordinates(lat, lon), defaultValue);
                        lon++;
                    }
                    deltaLon = 0;
                }
                var cellCount = Math.Min(cell.endLon, endLon) - lon;
                foreach(var ddp in cell.cell.GetPointsOnParallel(lat - cell.lat, lon - cell.lon, cellCount))
                {
                    yield return ddp;
                    lon++;
                }
            }
            while (lon < endLon)
            {
                yield return new DemDataPoint(Mapping.IndexToCoordinates(lat, lon), defaultValue);
                lon++;
            }
        }

        public DemDataCellBase<TPixel> ToDataCell()
        {
            var data = new TPixel[Mapping.PointsLat, Mapping.PointsLon];
            foreach(var cell in cellsData)
            {
                var targetLat = cell.lat;
                var sourceLat = 0;
                var countLat = cell.cell.PointsLat;

                var targetLon = cell.lon;
                var sourceLon = 0;
                var countLon = cell.cell.PointsLon;

                Cap(ref targetLat, ref sourceLat, ref countLat, Mapping.PointsLat);
                Cap(ref targetLon, ref sourceLon, ref countLon, Mapping.PointsLon);

                cell.cell.CopyData(sourceLat, sourceLon, countLat, countLon, data, targetLat, targetLon);
            }
            return DemDataCell.Create(Mapping.Start, Mapping.End, Mapping.RasterType, data);
        }

        private void Cap(ref int target, ref int source, ref int count, int max)
        {
            if (target < 0)
            {
                source -= target;
                target = 0;
            }
            if (target + count > max)
            {
                count = max - target;
            }
        }

        public IDemDataView CreateView(Coordinates start, Coordinates end)
        {
            return new DemDataView<TPixel>(cellsData.Select(c => c.cell), start, end); // Filter cells
        }
    }
}
