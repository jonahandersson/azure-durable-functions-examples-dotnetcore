using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Linq;

namespace FanOutFanInExample
{
    public static class GenerateOrderInvoiceActivity
    {
        [FunctionName("GenerateOrderInvoiceActivity")]
        public static string CalCulateOrderAmount([ActivityTrigger] List<double> orderAmounts, ILogger log)
        {
            var invoice = $"Invoice Orders Generated with total amount {orderAmounts.Sum()}.";
            log.LogInformation(invoice);
            return invoice;
        }
    }
}
