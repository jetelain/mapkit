using System.Collections.Generic;
using SimpleDEM.DataCells;

namespace SimpleDEM.Databases
{
    internal interface IDemStorage
    {
        List<DemDatabaseEntry> ReadIndex();

        IDemDataCell Load(DemDatabaseEntry entry);
    }
}