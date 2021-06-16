using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperMaxim.Logging
{
    internal class ConsoleLogger : ILogger
    {
        public ILoggerConfig Config { get; set; } = new ConsoleLoggerConfig();

        private const string LineSplitter = "/r/n";

        private bool IsEnabled => Config.IsEnabled;

        public ILogger LogInfo(string message)
        {
            if (!IsEnabled) return this;
            Debug.Log(message);
            return this;
        }

        public ILogger LogInfo(string format, string message)
        {
            if (!IsEnabled) return this;
            var msg = string.Format(format, message);
            return LogInfo(msg);
        }

        public ILogger LogInfo(string[] messages)
        {
            if (!IsEnabled) return this;
            var msg = string.Join(LineSplitter, messages);
            return LogInfo(msg);
        }

        public ILogger LogInfo(string format, string[] messages)
        {
            if (!IsEnabled) return this;
            var msg = string.Join(LineSplitter, messages);
            msg = string.Format(format, msg);
            return LogInfo(msg);
        }
        
        public ILogger LogInfo(string format, params object[] messages)
        {
            if (!IsEnabled) return this;
            Debug.LogFormat(format, messages);
            return this;
        }

        public ILogger LogWarning(string message)
        {
            if (!IsEnabled) return this;
            Debug.LogWarning(message);
            return this;
        }

        public ILogger LogWarning(string[] messages)
        {
            if (!IsEnabled) return this;
            var msg = string.Join(LineSplitter, messages);
            return LogWarning(msg);
        }

        public ILogger LogWarning(string format, string message)
        {
            if (!IsEnabled) return this;
            var msg = string.Format(format, message);
            return LogWarning(msg);
        }

        public ILogger LogWarning(string format, string[] messages)
        {
            if (!IsEnabled) return this;
            var msg = string.Join(LineSplitter, messages);
            msg = string.Format(format, msg);
            return LogWarning(msg);
        }

        public ILogger LogWarning(string format, params object[] messages)
        {
            if (!IsEnabled) return this;
            Debug.LogWarningFormat(format, messages);
            return this;
        }

        public ILogger LogError(string error)
        {
            if (!IsEnabled) return this;
            Debug.LogError(error);
            return this;
        }

        public ILogger LogError(Exception error)
        {
            if (!IsEnabled) return this;
            var msg = error.ToString();
            return LogInfo(msg);
        }

        public ILogger LogError(string[] errors)
        {
            if (!IsEnabled) return this;
            var error = string.Join(LineSplitter, errors);
            return LogError(error);
        }

        public ILogger LogError(string format, Exception error)
        {
            if (!IsEnabled) return this;
            var msg = string.Format(format, error);
            return LogError(msg);
        }

        public ILogger LogError(IEnumerable<Exception> errors)
        {
            if (!IsEnabled) return this;
            foreach (var error in errors)
            {
                LogError(error);
            }
            return this;
        }

        public ILogger LogError(string format, IEnumerable<Exception> errors)
        {
            if (!IsEnabled) return this;
            var msg = string.Join(LineSplitter, errors);
            LogError(string.Format(format, msg));
            return this;
        }

        public ILogger LogError(string format, params object[] errors)
        {
            if (!IsEnabled) return this;
            Debug.LogErrorFormat(format, errors);
            return this;
        }
    }
}