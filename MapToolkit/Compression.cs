using System;

namespace Pmad.Cartography
{
    public enum Compression
    {
        None = 0,
        ZSTD = 1,
        GZip = 2,
        [Obsolete("Use GZip instead.")]
        GZib = 2,
        Brotli = 3
    }
}