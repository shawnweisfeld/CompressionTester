using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CompressionTester
{
    public static class DotNetCompression
    {
        public static void CompressFiles(FileInfo[] files, string extension)
        {
            Console.WriteLine("DotNet Compressing");
            using (var timer = new Timer())
            {
                foreach (FileInfo fileToCompress in files)
                {
                    using (FileStream originalFileStream = fileToCompress.OpenRead())
                    using (FileStream compressedFileStream = File.Create($"{fileToCompress.FullName}.{extension}"))
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                        CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public static void DecompressFiles(FileInfo[] files, string extension)
        {
            Console.WriteLine("DotNet Decompressing");
            using (var timer = new Timer())
            {
                foreach (FileInfo fileToDecompress in files)
                {
                    using (FileStream originalFileStream = fileToDecompress.OpenRead())
                    using (FileStream decompressedFileStream = File.Create($"{fileToDecompress.FullName}.{extension}"))
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }

    }
}
