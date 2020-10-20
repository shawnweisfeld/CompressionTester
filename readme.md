# zCompressionTest

## What does this tool do

This tool will compress a folder full of files, and then decompress them.
It sends telemetry about the performance of the compression to an Azure Application Insights Instance.

## To run

1. Clone the Git Repo
1. Fill in the configuration settings in appsettings.json
    1. APPINSIGHTS_INSTRUMENTATIONKEY = this is the AI key that the logs will get sent to
    1. SourceFilesPath = this is local path to the folder that the source files are in
    1. DotNetCompressedPath = this is the local path to put the compressed files using the .NET algorithm in
    1. DotNetDecompressedPath = this is the local path to put the decompressed files using the .NET algorithm in
    1. OtherCompressedPath = this is the local path to put the compressed files using the other algorithm in
    1. OtherDecompressedPath = this is the local path to put the decompressed files using the other algorithm in
1. Update the code in the "OtherCompressionService.cs" file
    1. Replace line 34 and 53 with your compression algorithm
    1. Your code must include reading the source file from disk either compressing or decompressing it and then writing it back to disk
    1. All of your code must be between the line that news up PerfMon and when pm.Stop is called
    1. Use the .NET implementation as an example
