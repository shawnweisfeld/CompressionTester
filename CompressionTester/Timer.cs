using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CompressionTester
{
    public class Timer : IDisposable
    {
        Stopwatch _sw = new Stopwatch();

        public Timer()
        {
            _sw.Start();
        }

        public void Dispose()
        {
            _sw.Stop();
            Console.WriteLine($"\tDone in {_sw.ElapsedMilliseconds:N0} milliseconds");
        }
    }
}
