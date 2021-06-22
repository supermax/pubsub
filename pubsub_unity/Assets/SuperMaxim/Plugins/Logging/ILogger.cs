using System;
using System.Collections.Generic;

namespace SuperMaxim.Logging
{
    public interface ILogger
    {
        ILoggerConfig Config { get; set; }
        
        ILogger LogInfo(string message);
        
        ILogger LogInfo(string[] messages);
        
        ILogger LogInfo(string format, string message);

        ILogger LogInfo(string format, string[] messages);
        
        ILogger LogInfo(string format, params object[] messages);
        
        ILogger LogWarning(string message);
        
        ILogger LogWarning(string[] messages);
        
        ILogger LogWarning(string format, string message);

        ILogger LogWarning(string format, string[] messages);
        
        ILogger LogWarning(string format, params object[] messages);
        
        ILogger LogError(string error);
        
        ILogger LogError(Exception error);
        
        ILogger LogError(string[] errors);
        
        ILogger LogError(string format, Exception error);
        
        ILogger LogError(IEnumerable<Exception> errors);
        
        ILogger LogError(string format, IEnumerable<Exception> errors);
        
        ILogger LogError(string format, params object[] errors);
    }
}