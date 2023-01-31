using System;

namespace SuperMaxim.Core.Logging
{
    public interface ILoggerConfig
    {
        string Name { get; set; }
        
        LogTarget Target { get; set; }
        
        bool IsEnabled { get; set; }
    
        TimeSpan MessageTimeSpan { get; set; }
    }
}