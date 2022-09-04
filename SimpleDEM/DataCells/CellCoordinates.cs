namespace SimpleDEM.DataCells
{
    internal struct CellCoordinates
    {
        public CellCoordinates(int latitude, int longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public int Latitude { get; }

        public int Longitude { get; }
    }
}
