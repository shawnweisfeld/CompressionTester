﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace zCompressionWorker
{
    public class DotNetCompressionService
    {
        private TelemetryClient _telemetryClient;
        private readonly ILogger<DotNetCompressionService> _logger;
        private RunConfig _config;

        public DotNetCompressionService(ILogger<DotNetCompressionService> logger,
            TelemetryClient telemetryClient,
            RunConfig config)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _config = config;
        }

        public async Task CompressAsync(string source, string destination)
        {
            using (var dt = _telemetryClient.StartOperation<DependencyTelemetry>("DotNetCompression"))
            using (var pm = new PerfMon())
            using (FileStream sourceFileStream = File.OpenRead(source))
            using (FileStream destFileStream = File.Create(destination))
            using (GZipStream compressionStream = new GZipStream(destFileStream, CompressionMode.Compress))
            {
                await sourceFileStream.CopyToAsync(compressionStream);

                pm.Stop();

                dt.Telemetry.Type = "DotNetCompression";
                dt.Telemetry.Properties.Add("source", source);
                dt.Telemetry.Properties.Add("destination", destination);
                dt.Telemetry.Properties.Add("run", _config.RunID);
                dt.Telemetry.Properties.Add("used_cpu", pm.UsedProcessorTime.Ticks.ToString());
                dt.Telemetry.Properties.Add("used_memory", pm.UsedMemory.ToString());
            }
        }

        public async Task DecompressAsync(string source, string destination)
        {
            using (var dt = _telemetryClient.StartOperation<DependencyTelemetry>("DotNetDecompression"))
            using (var pm = new PerfMon())
            using (FileStream originalFileStream = File.OpenRead(source))
            using (FileStream decompressedFileStream = File.Create(destination))
            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
            {

                await decompressionStream.CopyToAsync(decompressedFileStream);

                pm.Stop();
                dt.Telemetry.Type = "DotNetDecompression";
                dt.Telemetry.Properties.Add("source", source);
                dt.Telemetry.Properties.Add("destination", destination);
                dt.Telemetry.Properties.Add("run", _config.RunID);
                dt.Telemetry.Properties.Add("used_cpu", pm.UsedProcessorTime.Ticks.ToString());
                dt.Telemetry.Properties.Add("used_memory", pm.UsedMemory.ToString());
            }
        }
    }
}
