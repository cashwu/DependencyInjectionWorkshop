internal class NlogAdapter
{
    public void LogFailedCount(string message)
    {
        var logger = NLog.LogManager.GetCurrentClassLogger();
        logger.Info(message);
    }
}