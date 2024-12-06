namespace Pmad.Cartography.DataCells
{
    internal interface IDemDataCellVisitor<U>
    {
        U Visit(DemDataCellPixelIsArea<short> cell);
        U Visit(DemDataCellPixelIsArea<ushort> cell);
        U Visit(DemDataCellPixelIsArea<float> cell);
        U Visit(DemDataCellPixelIsArea<double> cell);
        U Visit(DemDataCellPixelIsArea<int> cell);

        U Visit(DemDataCellPixelIsPoint<short> cell);
        U Visit(DemDataCellPixelIsPoint<ushort> cell);
        U Visit(DemDataCellPixelIsPoint<float> cell);
        U Visit(DemDataCellPixelIsPoint<double> cell);
        U Visit(DemDataCellPixelIsPoint<int> cell);
    }
}
