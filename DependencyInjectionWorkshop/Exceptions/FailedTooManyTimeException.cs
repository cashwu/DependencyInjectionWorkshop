using System;

namespace DependencyInjectionWorkshop.Exceptions
{
    public class FailedTooManyTimeException : Exception
    {
        public FailedTooManyTimeException(string accountId)
        {
        }
    }
}