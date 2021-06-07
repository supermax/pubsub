using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperMaxim.Logging
{
    internal class ConsoleLogger : ILogger
    {
        public ILoggerConfig Config { get; set; } = new ConsoleLoggerConfig();

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
            var msg = string.Join("/r/n", messages);
            return LogInfo(msg);
        }

        public ILogger LogInfo(string format, string[] messages)
        {
            if (!IsEnabled) return this;
            var msg = string.Join("/r/n", messages);
            msg = string.Format(format, msg);
            return LogInfo(msg);
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
            var msg = string.Join("/r/n", messages);
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
            var msg = string.Join("/r/n", messages);
            msg = string.Format(format, msg);
            return LogWarning(msg);
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

        public ILogger LogError(string format, Exception error)
        {
            if (!IsEnabled) return this;
            var msg = string.Format(format, error);
            return LogInfo(msg);
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
            foreach (var error in errors)
            {
                LogError(format, error);
            }
            return this;
        }
    }
}