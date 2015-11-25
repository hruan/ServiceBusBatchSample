using System;

namespace ProcessMonitorPoC.Common
{
    public sealed class ProcessInformation
    {
        public string Name { get; set; }
        public DateTime LastUsed { get; set; }
        public DateTime Installed { get; set; }
        public TimeSpan CpuTime { get; set; }
    }
}
