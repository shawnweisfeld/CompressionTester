using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace zCompressionWorker
{
    public class FileService
    {
        private readonly ILogger<FileService> _logger;
        private RunConfig _config;
        private const string ZIP_EXTENSION = ".gz";
        private TelemetryClient _telemetryClient;

        public FileService(ILogger<FileService> logger,
            TelemetryClient telemetryClient,
            RunConfig config)
        {
            _logger = logger;
            _config = config;
            _telemetryClient = telemetryClient;
        }

        public IEnumerable<string> ListFiles(string path, string filter = "*.*")
        {
            return Directory.GetFiles(path, filter, SearchOption.AllDirectories);
        }

        public void ResetFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in ListFiles(path))
            {
                File.Delete(file);
            }
        }

        public string GetDestinationZipFileName(string file, string sourcePath, string destPath)
        {
            return file.Replace(sourcePath, destPath) + ZIP_EXTENSION;
        }

        public string GetDestinationUnZipFileName(string file, string compressedPath, string decompressedPath)
        {
            file = file.Replace(compressedPath, decompressedPath);

            if (file.EndsWith(ZIP_EXTENSION))
                file = file.Substring(0, file.Length - ZIP_EXTENSION.Length);

            return file;
        }

        public long DirectorySize(string path)
        {
            return Directory.GetFiles(path).Select(x => new FileInfo(x)).Sum(x => x.Length);
        }

        public bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;

            if (!File.Exists(file1) || !File.Exists(file2))
                return false;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            using (var fs1 = new FileStream(file1, FileMode.Open))
            using (var fs2 = new FileStream(file2, FileMode.Open))
            {
                // Check the file sizes. If they are not the same, the files
                // are not the same.
                if (fs1.Length != fs2.Length)
                {
                    // Close the file
                    fs1.Close();
                    fs2.Close();

                    // Return false to indicate files are different
                    return false;
                }

                // Read and compare a byte from each file until either a
                // non-matching set of bytes is found or until the end of
                // file1 is reached.
                do
                {
                    // Read one byte from each file.
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));

            }

            // Return the success of the comparison. "file1byte" is
            // equal to "file2byte" at this point only if the files are
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        public void LogFileCompare(string source, string dest)
        {
            if (!FileCompare(source, dest))
            {
                _logger.LogError($"Invalid Compression: {source} -> {dest}");

                var et = new ExceptionTelemetry(new Exception("Invalid DotNet Compression"));
                et.Properties.Add("source", source);
                et.Properties.Add("destination", dest);
                et.Properties.Add("run", _config.RunID);

                _telemetryClient.TrackException(et);
            }
        }
    }

}
