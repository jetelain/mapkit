using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleDEM.DataCells.PixelFormats
{
    internal sealed class Int16Pixel : DemPixel<short>
    {
        public override short NoValue => short.MaxValue;

        public override short Average(IEnumerable<short> value)
        {
            return (short)value.Cast<int>().Average();
        }

        public override bool IsNoValue(short value)
        {
            return value == NoValue;
        }

        public override double ToDouble(short value)
        {
            if (IsNoValue(value))
            {
                return double.NaN;
            }
            return value;
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsArea<short> cell)
        {
            return visitor.Visit(cell);
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsPoint<short> cell)
        {
            return visitor.Visit(cell);
        }
    }
}
