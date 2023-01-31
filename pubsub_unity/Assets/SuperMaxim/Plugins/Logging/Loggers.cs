namespace SuperMaxim.Core.Logging
{
    public static class Loggers
    {
        public static ILogger Console { get; } = new UnityConsoleLogger();
    }
}
