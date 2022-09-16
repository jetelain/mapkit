using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.DataCells.PixelFormats
{
    internal sealed class DoublePixel : DemPixel<double>
    {
        public override double NoValue => double.NaN;

        public override double Average(IEnumerable<double> value)
        {
            return value.Average();
        }

        public override bool IsNoValue(double value)
        {
            return double.IsNaN(value);
        }

        public override double ToDouble(double value)
        {
            return value;
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsArea<double> cell)
        {
            return visitor.Visit(cell);
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsPoint<double> cell)
        {
            return visitor.Visit(cell);
        }
    }
}
