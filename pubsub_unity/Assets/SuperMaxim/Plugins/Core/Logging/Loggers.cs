namespace SuperMaxim.Core.Logging
{
    public static class Loggers
    {
        public static ILogger Console { get; } 
#if UNITY
                = new UnityConsoleLogger();
#else
                = new DebugConsoleLogger();
#endif
    }
}