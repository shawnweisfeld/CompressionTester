using System;
using System.Collections.Generic;
using System.Text;

namespace zCompressionWorker
{
    public class RunConfig
    {
        public string SourceFilesPath { get; set; }
        public string DotNetCompressedPath { get; set; }
        public string DotNetDecompressedPath { get; set; }
        public string OtherCompressedPath { get; set; }
        public string OtherDecompressedPath { get; set; }
        public string RunID { get; set; }

    }
}
