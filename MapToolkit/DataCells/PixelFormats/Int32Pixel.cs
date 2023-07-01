using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.DataCells.PixelFormats
{
    internal sealed class Int32Pixel : DemPixel<int>
    {
        public override int NoValue => int.MaxValue;

        public override int Average(IEnumerable<int> value)
        {
            return (int)value.Average();
        }

        public override bool IsNoValue(int value)
        {
            return value == NoValue;
        }

        public override double ToDouble(int value)
        {
            if (IsNoValue(value))
            {
                return double.NaN;
            }
            return value;
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsArea<int> cell)
        {
            return visitor.Visit(cell);
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsPoint<int> cell)
        {
            return visitor.Visit(cell);
        }

        public override int FromDouble(double value)
        {
            if (double.IsNaN(value))
            {
                return NoValue;
            }
            return (int)value;
        }
    }
}
