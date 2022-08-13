using System;
using System.IO;
using System.IO.Compression;

namespace SimpleDEM
{
    internal class CompressionHelper
    {
        public const string ExtensionGZip = ".gz";
        public const string ExtensionZStd = ".zst";
        public const string ExtensionBrotli = ".bt";
        public const string ExtensionZip = ".zip";

        internal static string GetExtension(string filename)
        {
            if (filename.EndsWith(ExtensionGZip, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetExtension(filename.Substring(0, filename.Length - ExtensionGZip.Length)).ToLowerInvariant();
            }
            if (filename.EndsWith(ExtensionZStd, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetExtension(filename.Substring(0, filename.Length - ExtensionZStd.Length)).ToLowerInvariant();
            }
            if (filename.EndsWith(ExtensionBrotli, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetExtension(filename.Substring(0, filename.Length - ExtensionBrotli.Length)).ToLowerInvariant();
            }
            if (filename.EndsWith(ExtensionZip, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetExtension(filename.Substring(0, filename.Length - ExtensionZip.Length)).ToLowerInvariant();
            }
            return Path.GetExtension(filename).ToLowerInvariant();
        }

        internal static T ReadSeekable<T>(string filename, Func<Stream, T> load)
        {
            if (filename.EndsWith(ExtensionGZip,   StringComparison.OrdinalIgnoreCase) ||
                filename.EndsWith(ExtensionZStd,   StringComparison.OrdinalIgnoreCase) ||
                filename.EndsWith(ExtensionBrotli, StringComparison.OrdinalIgnoreCase) ||
                filename.EndsWith(ExtensionZip,    StringComparison.OrdinalIgnoreCase))
            {
                var ms = Read(filename, stream =>
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return ms;
                });
                return load(ms);
            }

            using (var stream = new BufferedStream(File.OpenRead(filename)))
            {
                return load(stream);
            }
        }

        internal static T Read<T>(string filename, Func<Stream,T> load)
        {
            using (var stream = File.OpenRead(filename))
            {
                if (filename.EndsWith(ExtensionGZip, StringComparison.OrdinalIgnoreCase))
                {
                    using (var compressed = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        return load(compressed);
                    }
                }
                if (filename.EndsWith(ExtensionZStd, StringComparison.OrdinalIgnoreCase))
                {
                    using (var compressed = new ZstdSharp.DecompressionStream(stream))
                    {
                        return load(compressed);
                    }
                }
                if (filename.EndsWith(ExtensionBrotli, StringComparison.OrdinalIgnoreCase))
                {
                    using (var compressed = new BrotliStream(stream, CompressionMode.Decompress))
                    {
                        return load(compressed);
                    }
                }
                if (filename.EndsWith(ExtensionZip, StringComparison.OrdinalIgnoreCase))
                {
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        if (archive.Entries.Count > 1)
                        {
                            throw new IOException($"'{filename}' has multiple entries. Only one entry is allowed.");
                        }
                        using (var firstFile = archive.Entries[0].Open())
                        {
                            return load(firstFile);
                        }
                    }
                }
                using(var buffer = new BufferedStream(stream))
                {
                    return load(buffer);
                }
            }
        }


    }
}
