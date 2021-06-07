using System;

namespace SuperMaxim.Logging
{
    [Flags]
    public enum LogTarget
    {
        Console,
        File,
        Network
    }
}