using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CompressionTester
{
    public static class OtherCompression
    {
        public static void CompressFiles(FileInfo[] files, string extension)
        {
            Console.WriteLine("Other Compressing");
            using (var timer = new Timer())
            {
                //TODO: not implemented
            }
        }

        public static void DecompressFiles(FileInfo[] files, string extension)
        {
            Console.WriteLine("Other Decompressing");
            using (var timer = new Timer())
            {
                //TODO: not implemted
            }
        }
    }
}
