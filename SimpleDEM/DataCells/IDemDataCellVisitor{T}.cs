namespace SimpleDEM.DataCells
{
    internal interface IDemDataCellVisitor<U>
    {
        U Visit(DemDataCellPixelIsArea<short> cell);
        U Visit(DemDataCellPixelIsArea<ushort> cell);
        U Visit(DemDataCellPixelIsArea<float> cell);
        U Visit(DemDataCellPixelIsArea<double> cell);

        U Visit(DemDataCellPixelIsPoint<short> cell);
        U Visit(DemDataCellPixelIsPoint<ushort> cell);
        U Visit(DemDataCellPixelIsPoint<float> cell);
        U Visit(DemDataCellPixelIsPoint<double> cell);
    }
}
