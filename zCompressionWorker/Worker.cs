using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace zCompressionWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private TelemetryClient _telemetryClient;
        private RunConfig _config;
        private FileService _fileService;
        private DotNetCompressionService _dotNetCompressionService;
        private OtherCompressionService _otherCompressionService;

        public Worker(ILogger<Worker> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            TelemetryClient telemetryClient,
            RunConfig config,
            FileService fileService,
            DotNetCompressionService dotNetCompressionService,
            OtherCompressionService otherCompressionService
            )
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _telemetryClient = telemetryClient;
            _config = config;
            _fileService = fileService;
            _dotNetCompressionService = dotNetCompressionService;
            _otherCompressionService = otherCompressionService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Worker Cancelling");
            });

            try
            {
                await RunAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation Canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception");
            }
            finally
            {
                _logger.LogInformation("Flusing App Insights");
                _telemetryClient.Flush();
                Task.Delay(5000).Wait();

                _hostApplicationLifetime.StopApplication();
            }
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            //log the current configuration
            _logger.LogProperties(_config);

            //Compress everything
            _fileService.ResetFolder(_config.DotNetCompressedPath);
            _fileService.ResetFolder(_config.OtherCompressedPath);
            foreach (var file in _fileService.ListFiles(_config.SourceFilesPath))
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    await _dotNetCompressionService.CompressAsync(file,
                        _fileService.GetDestinationZipFileName(file, _config.SourceFilesPath, _config.DotNetCompressedPath));

                    await _otherCompressionService.CompressAsync(file,
                        _fileService.GetDestinationZipFileName(file, _config.SourceFilesPath, _config.OtherCompressedPath));
                }
            }

            //Decompress everything with the .NET Decompressor
            _fileService.ResetFolder(_config.DotNetDecompressedPath);
            foreach (var file in _fileService.ListFiles(_config.DotNetCompressedPath))
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    await _dotNetCompressionService.DecompressAsync(file,
                        _fileService.GetDestinationUnZipFileName(file, _config.DotNetCompressedPath, _config.DotNetDecompressedPath));
                }
            }

            _fileService.ResetFolder(_config.OtherDecompressedPath);
            foreach (var file in _fileService.ListFiles(_config.OtherCompressedPath))
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    await _otherCompressionService.DecompressAsync(file,
                        _fileService.GetDestinationUnZipFileName(file, _config.OtherCompressedPath, _config.OtherDecompressedPath));
                }
            }

            //Check that the original and zip files match
            foreach (var file in _fileService.ListFiles(_config.SourceFilesPath))
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _fileService.LogFileCompare(file, _fileService.GetDestinationUnZipFileName(file, _config.SourceFilesPath, _config.DotNetDecompressedPath));
                    _fileService.LogFileCompare(file, _fileService.GetDestinationUnZipFileName(file, _config.SourceFilesPath, _config.OtherDecompressedPath));
                }
            }

            //Measure the effectivness of the compression
            double origSize = _fileService.DirectorySize(_config.SourceFilesPath);
            double dotNetCompressedSize = _fileService.DirectorySize(_config.DotNetCompressedPath);
            double dotNetCompresionPct = 1 - (dotNetCompressedSize / origSize);
            double otherCompressedSize = _fileService.DirectorySize(_config.OtherCompressedPath);
            double otherCompresionPct = 1 - (otherCompressedSize / origSize);


            _logger.LogInformation($"DotNetCompression {dotNetCompresionPct:P2}");
            var mt = new MetricTelemetry("DotNetCompression", dotNetCompresionPct);
            mt.Properties.Add("run", _config.RunID);
            _telemetryClient.TrackMetric(mt);


            _logger.LogInformation($"OtherCompression {otherCompresionPct:P2}");
            mt = new MetricTelemetry("OtherCompression", otherCompresionPct);
            mt.Properties.Add("run", _config.RunID);
            _telemetryClient.TrackMetric(mt);
        }
    }
}
