using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FanOutFanInExample
{
    public static class StarterFunction
    {
        [FunctionName("StarterFunction")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,  ILogger log)
        {
            var orderIds = Enumerable.Range(0, 10);

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("OrchestratorFunction", orderIds.ToList());
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
