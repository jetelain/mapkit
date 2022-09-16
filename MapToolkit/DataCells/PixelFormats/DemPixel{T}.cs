using System;
using System.Collections.Generic;

namespace MapToolkit.DataCells.PixelFormats
{
    internal abstract class DemPixel<T> : DemPixel 
        where T : unmanaged
    {
        public abstract T NoValue { get; }

        public abstract bool IsNoValue(T value);

        public abstract T Average(IEnumerable<T> value);

        public abstract double ToDouble(T value);

        public abstract U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsArea<T> cell);

        public abstract U Accept<U>(IDemDataCellVisitor<U> visitor, DemDataCellPixelIsPoint<T> cell);
    }
}