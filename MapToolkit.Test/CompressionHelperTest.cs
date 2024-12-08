using System;
using System.IO;
using System.IO.Compression;
using Xunit;
using Pmad.Cartography;

namespace Pmad.Cartography.Test
{
    public class CompressionHelperTest
    {
        [Fact]
        public void GetExtension_FromFilename()
        {
            Assert.Equal(".txt", CompressionHelper.GetExtension("file.txt.gz"));
            Assert.Equal(".txt", CompressionHelper.GetExtension("file.txt.zst"));
            Assert.Equal(".txt", CompressionHelper.GetExtension("file.txt.bt"));
            Assert.Equal(".txt", CompressionHelper.GetExtension("file.txt.zip"));
            Assert.Equal(".txt", CompressionHelper.GetExtension("file.txt"));
        }

        [Fact]
        public void GetExtension_FromCompression()
        {
            Assert.Equal(".zst", CompressionHelper.GetExtension(Compression.ZSTD));
            Assert.Equal(".gz", CompressionHelper.GetExtension(Compression.GZib));
            Assert.Equal(".bt", CompressionHelper.GetExtension(Compression.Brotli));
            Assert.Equal(string.Empty, CompressionHelper.GetExtension(Compression.None));
        }

        [Fact]
        public void GetFileName_RemovesExtension()
        {
            Assert.Equal("file.txt", CompressionHelper.GetFileName("file.txt.gz"));
            Assert.Equal("file.txt", CompressionHelper.GetFileName("file.txt.zst"));
            Assert.Equal("file.txt", CompressionHelper.GetFileName("file.txt.bt"));
            Assert.Equal("file.txt", CompressionHelper.GetFileName("file.txt.zip"));
            Assert.Equal("file.txt", CompressionHelper.GetFileName("file.txt"));
        }

        [Fact]
        public void ReadSeekable_ReadsCompressedFile_Compressed()
        {
            var filename = WriteCompressedFile(Compression.GZib);
            var result = CompressionHelper.ReadSeekable(filename, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            });
            Assert.Equal("test content", result);
            File.Delete(filename);
        }

        [Fact]
        public void ReadSeekable_ReadsCompressedFile_None()
        {
            var filename = WriteCompressedFile(Compression.None);
            var result = CompressionHelper.ReadSeekable(filename, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            });
            Assert.Equal("test content", result);
            File.Delete(filename);
        }

        [Fact]
        public void Read_ReadsCompressedFile_GZib()
        {
            var filename = WriteCompressedFile(Compression.GZib);
            var result = CompressionHelper.Read(filename, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            });
            Assert.Equal("test content", result);
            File.Delete(filename);
        }

        [Fact]
        public void Read_ReadsCompressedFile_None()
        {
            var filename = WriteCompressedFile(Compression.None);
            var result = CompressionHelper.Read(filename, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            });
            Assert.Equal("test content", result);
            File.Delete(filename);
        }

        [Fact]
        public void Read_ReadsCompressedFile_ZSTD()
        {
            var filename = WriteCompressedFile(Compression.ZSTD);
            var result = CompressionHelper.Read(filename, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            });
            Assert.Equal("test content", result);
            File.Delete(filename);
        }

        [Fact]
        public void Read_ReadsCompressedFile_Brotli()
        {
            var filename = WriteCompressedFile(Compression.Brotli);
            var result = CompressionHelper.Read(filename, stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            });
            Assert.Equal("test content", result);
            File.Delete(filename);
        }

        [Fact]
        public void GetSize_ReturnsCorrectSize_GZip()
        {
            var filename = WriteCompressedFile(Compression.GZib);
            var size = CompressionHelper.GetSize(filename);
            Assert.Equal(12, size); // "test content" length
            File.Delete(filename);
        }

        [Fact]
        public void GetSize_ReturnsCorrectSize_Brotli()
        {
            var filename = WriteCompressedFile(Compression.Brotli);
            var size = CompressionHelper.GetSize(filename);
            Assert.Equal(12, size); // "test content" length
            File.Delete(filename);
        }

        [Fact]
        public void GetSize_ReturnsCorrectSize_ZSTD()
        {
            var filename = WriteCompressedFile(Compression.ZSTD);
            var size = CompressionHelper.GetSize(filename);
            Assert.Equal(12, size); // "test content" length
            File.Delete(filename);
        }

        [Fact]
        public void GetSize_ReturnsCorrectSize_None()
        {
            var filename = WriteCompressedFile(Compression.None);
            var size = CompressionHelper.GetSize(filename);
            Assert.Equal(12, size); // "test content" length
            File.Delete(filename);
        }

        private static string WriteCompressedFile(Compression compression)
        {
            var filename = "test.txt" + CompressionHelper.GetExtension(compression);
            CompressionHelper.Write(filename, compression, stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("test content");
                }
            });
            return filename;
        }
    }
}
