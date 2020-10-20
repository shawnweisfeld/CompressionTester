using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace zCompressionWorker
{
    public class PerfMon : IDisposable
    {
        private TimeSpan _startcpu;
        private TimeSpan _endcpu;
        private Process _process;
        private long _usedmemory;

        public TimeSpan UsedProcessorTime 
        {
            get 
            {
                return _endcpu - _startcpu;
            }
        }

        public long UsedMemory
        {
            get
            {
                return _usedmemory;
            }
        }

        public PerfMon()
        {
            _process = Process.GetCurrentProcess();
            Start();
        }

        public void Start()
        {
            _startcpu = _process.TotalProcessorTime;
        }


        public void Stop()
        {
            _endcpu = _process.TotalProcessorTime;
            _usedmemory = _process.WorkingSet64;
        }

        public void Dispose()
        {
            
        }
    }
}
