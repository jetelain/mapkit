using System;
using System.Threading;
using SimpleDEM.DataCells;

namespace SimpleDEM.Databases
{
    internal class DemDatabaseEntry
    {
        internal DemDatabaseEntry(string path, IDemDataCellMetadata metadata)
        {
            Path = path;
            Metadata = metadata;
        }

        public string Path { get; }

        public IDemDataCellMetadata Metadata { get; }

        public IDemDataCell? Data { get; private set; }

        private long lastAccess;

        public long LastAccess
        {
            get { return Interlocked.Read(ref lastAccess); }
            private set { Interlocked.Exchange(ref lastAccess, lastAccess = value); }
        }

        public bool Contains(GeodeticCoordinates coordinates)
        {
            return Metadata.Start.Latitude <= coordinates.Latitude &&
                    Metadata.End.Latitude >= coordinates.Latitude &&
                    Metadata.Start.Longitude <= coordinates.Longitude &&
                    Metadata.End.Longitude >= coordinates.Longitude;
        }

        private void SetLastAccess()
        {
            LastAccess = DateTime.UtcNow.Ticks;
        }

        public IDemDataCell Load(IDemStorage storage)
        {
            var data = storage.Load(this);
            SetLastAccess();
            Data = data;
            return data;
        }

        public IDemDataCell? PickData()
        {
            var data = Data;
            if (data != null)
            {
                SetLastAccess();
            }
            return data;
        }

        public int UnLoad()
        {
            var bytes = Data?.SizeInBytes ?? 0;
            Data = null;
            return bytes;
        }
    }
}
