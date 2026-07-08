#nullable enable
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Game
{
    internal static class SaveText
    {
        public static string Compress(string json)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                gzip.Write(bytes, 0, bytes.Length);
            }

            return Convert.ToBase64String(output.ToArray());
        }

        public static string Expand(string text)
        {
            using var input = new MemoryStream(Convert.FromBase64String(text));
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
