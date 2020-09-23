using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CompressionTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            DirectoryInfo directorySelected = new DirectoryInfo("temp");
            int numOfFiles = 5;
            int minFileSize = 100;
            int maxFileSize = 200;
            int blockSize = 8 * 1024;
            int blocksPerMb = (1024 * 1024) / blockSize;

            if (directorySelected.Exists)
                directorySelected.Delete(true);

            directorySelected.Create();

            //generate a folder of random data
            Console.WriteLine("Creating Random Sample Data");
            using (var timer = new Timer())
            {
                for (int fileNum = 0; fileNum < numOfFiles; fileNum++)
                {
                    string fileName = Path.Combine(directorySelected.FullName, $"{Guid.NewGuid().ToString().ToLower()}.dat");
                    int fileSizeInMb = random.Next(minFileSize, maxFileSize);

                    using (FileStream fs = File.OpenWrite(fileName))
                    {
                        byte[] block = new byte[blockSize];
                        for (int i = 0; i < fileSizeInMb * blocksPerMb; i++)
                        {
                            random.NextBytes(block);
                            fs.Write(block, 0, block.Length);
                        }
                    }
                }
            }



            DotNetCompression.CompressFiles(directorySelected.GetFiles("*.dat"), "dotnet.gz");
            DotNetCompression.DecompressFiles(directorySelected.GetFiles("*.dotnet.gz"), "undat");

            OtherCompression.CompressFiles(directorySelected.GetFiles("*.dat"), "other.gz");
            OtherCompression.DecompressFiles(directorySelected.GetFiles("*.other.gz"), "undat");

            //check the input/output files to make sure that all the bytes are the same
            foreach (var origFileName in directorySelected.GetFiles("*.dat"))
            {
                if (!FileHelper.FileCompare(origFileName.FullName, $"{origFileName.FullName}.dotnet.gz.undat"))
                {
                    Console.WriteLine($"DotNet Mismatch = {origFileName.Name}");
                }

                if (!FileHelper.FileCompare(origFileName.FullName, $"{origFileName.FullName}.other.gz.undat"))
                {
                    Console.WriteLine($"Other Mismatch = {origFileName.Name}");
                }
            }

            Console.WriteLine($"DotNet Compressed Size: {directorySelected.GetFiles("*.dotnet.gz").Sum(x => x.Length):N0}");
            Console.WriteLine($"Other  Compressed Size: {directorySelected.GetFiles("*.other.gz").Sum(x => x.Length):N0}");

            Console.WriteLine("Done!");
        }

    }
}
