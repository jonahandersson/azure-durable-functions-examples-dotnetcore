using System;

namespace AzureDurableFunctions.ErrorHandlingExtensions
{
    public class RetryOptionsAsync
    {
        public DateTimeOffset FirstRetryOptions { get; set; }
        public int MaxNumberOfAtempts { get; set; }
    }
}
