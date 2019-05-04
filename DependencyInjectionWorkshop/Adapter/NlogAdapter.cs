using System;

namespace DependencyInjectionWorkshop.Adapter
{
    public class NlogAdapter : ILogger
    {
        public virtual void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
    
    public class ConsoleAdapter : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message); 
        }
    }
}