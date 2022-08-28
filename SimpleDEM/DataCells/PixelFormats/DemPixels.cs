using System;
using System.Collections.Generic;

namespace SimpleDEM.DataCells.PixelFormats
{
    internal static class DemPixels
    {
        private static readonly Dictionary<Type, DemPixel> PixelImplementations = new Dictionary<Type, DemPixel>()
        {
            { typeof(float), new FloatPixel() },
            { typeof(short), new Int16Pixel() },
            { typeof(ushort), new UInt16Pixel() },
            { typeof(double), new DoublePixel() },
        };

        internal static DemPixel<T> Get<T>() 
            where T : unmanaged
        {
            return (DemPixel<T>)PixelImplementations[typeof(T)];
        }
    }
}