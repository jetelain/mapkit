using System;
using System.IO;
using System.IO.Compression;

namespace MapToolkit
{
    public class CompressionHelper
    {
        public const string ExtensionGZip = ".gz";
        public const string ExtensionZStd = ".zst";
        public const string ExtensionBrotli = ".bt";
        public const string ExtensionZip = ".zip";

        public static string GetExtension(string filename)
        {
            return Path.GetExtension(GetFileName(filename)).ToLowerInvariant();
        }

        public static string GetExtension(Compression compression)
        {
            switch (compression)
            {
                case Compression.ZSTD:
                    return ExtensionZStd;
                case Compression.GZib:
                    return ExtensionGZip;
                case Compression.Brotli:
                    return ExtensionBrotli;
                case Compression.None:
                default:
                    return string.Empty;
            }
        }

        public static string GetFileName(string filename)
        {
            if (filename.EndsWith(ExtensionGZip, StringComparison.OrdinalIgnoreCase))
            {
                return filename.Substring(0, filename.Length - ExtensionGZip.Length);
            }
            if (filename.EndsWith(ExtensionZStd, StringComparison.OrdinalIgnoreCase))
            {
                return filename.Substring(0, filename.Length - ExtensionZStd.Length);
            }
            if (filename.EndsWith(ExtensionBrotli, StringComparison.OrdinalIgnoreCase))
            {
                return filename.Substring(0, filename.Length - ExtensionBrotli.Length);
            }
            if (filename.EndsWith(ExtensionZip, StringComparison.OrdinalIgnoreCase))
            {
                return filename.Substring(0, filename.Length - ExtensionZip.Length);
            }
            return filename;
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

        public static T Read<T>(string filename, Func<Stream,T> load)
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

        public static long GetSize(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                if (filename.EndsWith(ExtensionGZip, StringComparison.OrdinalIgnoreCase))
                {
                    using (var compressed = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        return GetSize(compressed);
                    }
                }
                if (filename.EndsWith(ExtensionZStd, StringComparison.OrdinalIgnoreCase))
                {
                    using (var compressed = new ZstdSharp.DecompressionStream(stream))
                    {
                        return GetSize(compressed);
                    }
                }
                if (filename.EndsWith(ExtensionBrotli, StringComparison.OrdinalIgnoreCase))
                {
                    using (var compressed = new BrotliStream(stream, CompressionMode.Decompress))
                    {
                        return GetSize(compressed);
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
                        return archive.Entries[0].Length;
                    }
                }
                return stream.Length;
            }
        }

        private static long GetSize(Stream compressed)
        {
            var buffer = new byte[4096];
            var total = 0;
            int size;
            while((size = compressed.Read(buffer, 0, buffer.Length)) > 0)
            {
                total += size;
            }
            return total;
        }

        public static void Read(string filename, Action<Stream> load)
        {
            Read(filename, s => { load(s); return 0; }); 
        }

        public static void Write(string target, Compression compression, Action<Stream> write)
        {
            using (var stream = File.Create(target))
            {
                switch (compression)
                {
                    case Compression.ZSTD:
                        using (var compressed = new ZstdSharp.CompressionStream(stream, 22))
                        {
                            write(compressed);
                        }
                        break;
                    case Compression.GZib:
                        using (var compressed = new GZipStream(stream, CompressionMode.Compress))
                        {
                            write(compressed);
                        }
                        break;
                    case Compression.Brotli:
                        using (var compressed = new BrotliStream(stream, CompressionMode.Compress))
                        {
                            write(compressed);
                        }
                        break;
                    case Compression.None:
                    default:
                        write(stream);
                        break;
                }
            }
        }

    }

}