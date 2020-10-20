using Microsoft.ApplicationInsights;
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
    public class OtherCompressionService
    {
        private TelemetryClient _telemetryClient;
        private readonly ILogger<OtherCompressionService> _logger;
        private RunConfig _config;

        public OtherCompressionService(ILogger<OtherCompressionService> logger,
            TelemetryClient telemetryClient,
            RunConfig config)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _config = config;
        }

        public async Task CompressAsync(string source, string destination)
        {
            using (var dt = _telemetryClient.StartOperation<DependencyTelemetry>("OtherCompression"))
            using (var pm = new PerfMon())
            {
                //TODO: Replace this line of code with the call to your compression SDK
                await Task.Run(() => File.Copy(source, destination));

                pm.Stop();

                dt.Telemetry.Type = "OtherCompression";
                dt.Telemetry.Properties.Add("source", source);
                dt.Telemetry.Properties.Add("destination", destination);
                dt.Telemetry.Properties.Add("run", _config.RunID);
                dt.Telemetry.Properties.Add("used_cpu", pm.UsedProcessorTime.Ticks.ToString());
                dt.Telemetry.Properties.Add("used_memory", pm.UsedMemory.ToString());
            }
        }

        public async Task DecompressAsync(string source, string destination)
        {
            using (var dt = _telemetryClient.StartOperation<DependencyTelemetry>("OtherDecompression"))
            using (var pm = new PerfMon())
            {
                //TODO: Replace this line of code with the call to your compression SDK
                await Task.Run(() => File.Copy(source, destination));

                pm.Stop();

                dt.Telemetry.Type = "OtherDecompression";
                dt.Telemetry.Properties.Add("source", source);
                dt.Telemetry.Properties.Add("destination", destination);
                dt.Telemetry.Properties.Add("run", _config.RunID);
                dt.Telemetry.Properties.Add("used_cpu", pm.UsedProcessorTime.Ticks.ToString());
                dt.Telemetry.Properties.Add("used_memory", pm.UsedMemory.ToString());
            }
        }
    }
}
