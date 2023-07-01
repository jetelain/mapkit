using System;
using System.Collections.Generic;
using System.Linq;

namespace MapToolkit.DataCells.PixelFormats
{
    internal sealed class FloatPixel : DemPixel<float>
    {
        public override float NoValue => float.NaN;

        public override float Average(IEnumerable<float> value)
        {
            return value.Average();
        }

        public override bool IsNoValue(float value)
        {
            return float.IsNaN(value);
        }

        public override double ToDouble(float value)
        {
            return (double)value;
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsArea<float> cell)
        {
            return visitor.Visit(cell);
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsPoint<float> cell)
        {
            return visitor.Visit(cell);
        }

        public override float FromDouble(double value)
        {
            return (float)value;
        }
    }
}
