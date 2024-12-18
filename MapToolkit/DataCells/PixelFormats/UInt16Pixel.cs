﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Pmad.Cartography.DataCells.PixelFormats
{
    internal sealed class UInt16Pixel : DemPixel<ushort>
    {
        public override ushort NoValue => (ushort)short.MaxValue;

        public override ushort Average(IEnumerable<ushort> value)
        { 
            return (ushort) value.Select(v => (int)v).Average();
        }

        public override bool IsNoValue(ushort value)
        {
            return value >= NoValue;
        }

        public override double ToDouble(ushort value)
        {
            if (IsNoValue(value))
            {
                return double.NaN;
            }
            return value;
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsArea<ushort> cell)
        {
            return visitor.Visit(cell);
        }

        public override U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsPoint<ushort> cell)
        {
            return visitor.Visit(cell);
        }

        public override ushort FromDouble(double value)
        {
            if(double.IsNaN(value))
            {
                return NoValue;
            }
            return (ushort)value;
        }
    }
}
