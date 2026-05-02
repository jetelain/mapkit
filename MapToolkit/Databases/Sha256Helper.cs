using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pmad.Cartography.Databases
{
    public static class Sha256Helper
    {
        public static async Task<string> ComputeHexAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
            return BytesToHex(hash);
        }

        public static async Task<bool> VerifyAsync(string filePath, string expectedHex, CancellationToken cancellationToken = default)
        {
            return string.Equals(await ComputeHexAsync(filePath, cancellationToken).ConfigureAwait(false), expectedHex, StringComparison.OrdinalIgnoreCase);
        }

        private static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
