using System;

namespace SuperMaxim.Core.Logging
{
    [Flags]
    public enum LogTarget
    {
        Console,
        File,
        Network
    }
}