using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Pmad.Cartography.Databases;

namespace Pmad.Cartography.Test.Databases
{
    public class Sha256HelperTest
    {
        private static string CreateTempFileWithContent(byte[] content)
        {
            var path = Path.GetTempFileName();
            File.WriteAllBytes(path, content);
            return path;
        }

        private static string ComputeExpectedHex(byte[] content)
        {
            var hash = SHA256.HashData(content);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        [Fact]
        public async Task ComputeHex_ReturnsCorrectHash()
        {
            var content = "hello world"u8.ToArray();
            var file = CreateTempFileWithContent(content);
            try
            {
                var expected = ComputeExpectedHex(content);
                Assert.Equal(expected, await Sha256Helper.ComputeHexAsync(file));
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Fact]
        public async Task VerifyAsync_ReturnsTrue_WhenHashMatches()
        {
            var content = "hello world"u8.ToArray();
            var file = CreateTempFileWithContent(content);
            try
            {
                var hex = ComputeExpectedHex(content);
                Assert.True(await Sha256Helper.VerifyAsync(file, hex));
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Fact]
        public async Task VerifyAsync_ReturnsTrue_WhenHashMatchesUpperCase()
        {
            var content = "hello world"u8.ToArray();
            var file = CreateTempFileWithContent(content);
            try
            {
                var hex = ComputeExpectedHex(content).ToUpperInvariant();
                Assert.True(await Sha256Helper.VerifyAsync(file, hex));
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Fact]
        public async Task VerifyAsync_ReturnsFalse_WhenHashDoesNotMatch()
        {
            var content = "hello world"u8.ToArray();
            var file = CreateTempFileWithContent(content);
            try
            {
                Assert.False(await Sha256Helper.VerifyAsync(file, "0000000000000000000000000000000000000000000000000000000000000000"));
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}
