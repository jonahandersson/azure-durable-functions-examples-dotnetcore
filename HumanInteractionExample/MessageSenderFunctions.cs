using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace HumanInteractionExample
{
    public static class MessageSenderFunctions
    {
        [FunctionName("MessageSenderFunctions_Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var employees = new List<Employee>();
        
            var managers = new List<Manager>();
            var messagesToManagers = new List<string>();

            foreach (var manager in managers)
            {
                messagesToManagers.Add(await context.CallActivityAsync<string>("Activity_SendEmailToManagers", "London"));
            }
          
           //TODO: Output and log, do nóre activities
            return messagesToManagers;
        }

        [FunctionName("Activity_SendEmailToManagers")]
        public static string SendEmailToManagers([ActivityTrigger] string name, ILogger log)
        {
            //TODO 
            log.LogInformation($"sending email to manager name -  {name}.");
            return $"Hello Manager {name}!";
        }


        [FunctionName("Activity_SendEmailToEmployees")]
        public static string SendEmailToEmployeesUponManagerApproval([ActivityTrigger] string name, ILogger log)
        {
            //TODO 
            log.LogInformation($"Managers Approval Done. Sending email to employee name -  {name}.");
            return $"Hello Employee {name}!";
        }

        [FunctionName("MessageSenderFunctions_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("MessageSenderFunctions_Orchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}