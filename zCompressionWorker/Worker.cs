using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
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

        public Worker(ILogger<Worker> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            TelemetryClient telemetryClient,
            RunConfig config,
            FileService fileService,
            DotNetCompressionService dotNetCompressionService
            )
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _telemetryClient = telemetryClient;
            _config = config;
            _fileService = fileService;
            _dotNetCompressionService = dotNetCompressionService;
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
                _hostApplicationLifetime.StopApplication();
            }
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {

            //Compress everything with the .NET Compressor
            _fileService.ResetFolder(_config.DotNetCompressedPath);
            foreach (var file in _fileService.ListFiles(_config.SourceFilesPath))
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    await _dotNetCompressionService.CompressAsync(file,
                        _fileService.GetDestinationZipFileName(file, _config.SourceFilesPath, _config.DotNetCompressedPath));
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

            //Check that the original and zip files match
            foreach (var file in _fileService.ListFiles(_config.SourceFilesPath))
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    var destFile = _fileService.GetDestinationUnZipFileName(file, _config.SourceFilesPath, _config.DotNetDecompressedPath);
                    if (!_fileService.FileCompare(file, destFile))
                    {
                        _logger.LogError($"Invalid Compression: {destFile}");

                        var et = new ExceptionTelemetry(new Exception("Invalid DotNet Compression"));
                        et.Properties.Add("source", file);
                        et.Properties.Add("destination", destFile);
                        et.Properties.Add("run", _config.RunID);

                        _telemetryClient.TrackException(et);
                    }
                }
            }

            //Measure the effectivness of the compression
            double origSize = _fileService.DirectorySize(_config.SourceFilesPath);
            double dotNetCompressedSize = _fileService.DirectorySize(_config.DotNetCompressedPath);
            double dotNetCompresion = 1 - (dotNetCompressedSize / origSize);

            _logger.LogInformation($"DotNetCompression {dotNetCompresion:P2}");
            var mt = new MetricTelemetry("DotNetCompression", dotNetCompresion);
            mt.Properties.Add("run", _config.RunID);
            _telemetryClient.TrackMetric(mt);

        }
    }
}
