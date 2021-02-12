using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FanOutFanInExample
{
    public static class OrchestratorFunction
    {
        [FunctionName("OrchestratorFunction")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {

            var orders = context.GetInput<List<string>>();
            var orderTasks = new List<Task<double>>();
            for (int i = 0; i < orders.Count; i++)
            {
                var orderTaskToDo = context.CallActivityAsync<double>("CalculateOrderActivity",
                    orders[i]);
                orderTasks.Add(orderTaskToDo);
            }

            var orderAmounts = await Task.WhenAll(orderTasks);

            var invoiceOrderSummary = await context.CallActivityAsync<string>("GenerateOrderInvoiceActivity",
                  orderAmounts);

            log.LogInformation("Done summing up the orders. See logs");

            return invoiceOrderSummary;
        }


    }
}