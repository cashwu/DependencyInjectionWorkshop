using System;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedTooManyTimeException : Exception
    {
        public FailedTooManyTimeException(string accountId)
        {
        }
    }
}