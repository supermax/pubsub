using System;

namespace SuperMaxim.Logging
{
    public class ConsoleLoggerConfig : ILoggerConfig
    {
        public string Name { get; set; }
        
        public LogTarget Target { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public TimeSpan MessageTimeSpan { get; set; }

        public ConsoleLoggerConfig()
        {
            Name = "Console Logger";
            Target = LogTarget.Console;
            IsEnabled = true;
        }
    }
}