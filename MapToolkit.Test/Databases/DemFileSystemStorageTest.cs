using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Pmad.Cartography.Databases;
using Xunit;

namespace Pmad.Cartography.Test.Databases
{
    public class DemFileSystemStorageTest
    {
        private static string CreateTempFile(byte[] content)
        {
            var path = Path.GetTempFileName();
            File.WriteAllBytes(path, content);
            return path;
        }

        private static string ComputeExpectedHex(byte[] content)
        {
            return Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        }

        [Fact]
        public async Task GetSha256Async_ShouldReturnCorrectHash_WhenFileExists()
        {
            // Arrange
            var content = "hello dem"u8.ToArray();
            var file = CreateTempFile(content);
            var basePath = Path.GetDirectoryName(file)!;
            var fileName = Path.GetFileName(file);
            var storage = new DemFileSystemStorage(basePath);

            try
            {
                // Act
                var hash = await storage.GetSha256Async(fileName);

                // Assert
                Assert.NotNull(hash);
                Assert.Equal(ComputeExpectedHex(content), hash);
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Fact]
        public async Task GetSha256Async_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // Arrange
            var basePath = Path.GetTempPath();
            var storage = new DemFileSystemStorage(basePath);

            // Act
            var hash = await storage.GetSha256Async("nonexistent_file_that_does_not_exist.hgt.zst");

            // Assert
            Assert.Null(hash);
        }

        [Fact]
        public async Task GetSha256Async_ShouldReturnLowercaseHex()
        {
            // Arrange
            var content = "case check"u8.ToArray();
            var file = CreateTempFile(content);
            var basePath = Path.GetDirectoryName(file)!;
            var fileName = Path.GetFileName(file);
            var storage = new DemFileSystemStorage(basePath);

            try
            {
                // Act
                var hash = await storage.GetSha256Async(fileName);

                // Assert
                Assert.NotNull(hash);
                Assert.Equal(hash!.ToLowerInvariant(), hash);
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}
