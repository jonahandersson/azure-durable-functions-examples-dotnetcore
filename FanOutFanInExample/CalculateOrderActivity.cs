using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FanOutFanInExample
{
    public static class CalculateOrderActivity
    {       
        [FunctionName("CalculateOrderActivity")]
        public static double CalCulateOrderAmount([ActivityTrigger] string OrderId, ILogger log)
        {
            log.LogInformation($"Order calculation for {OrderId}.");
            return new Random().NextDouble();
        }

    }
}