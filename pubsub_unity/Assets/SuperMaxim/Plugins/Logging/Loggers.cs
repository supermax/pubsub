namespace SuperMaxim.Logging
{
    public static class Loggers
    {
        public static ILogger Console { get; } = new ConsoleLogger();
    }
}