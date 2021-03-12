using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AzureDurableFunctions.MailSender
{
    public static class DurableMailSender
    {
        [FunctionName("DurableMailSender_Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("DurableMailSender_MailSenderFunction")]
        public static string SendEmails([ActivityTrigger] string name, ILogger log)
        {
            //TODO 
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress("jonah@jonahandersson.tech", "SendGrid Test From APp"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress("cjonah@example.com", "Jane Doe"),
                new EmailAddress("anna@example.com", "Anna Lidman"),
                new EmailAddress("peter@example.com", "Peter Saddow")
            };
            msg.AddTos(recipients);

            msg.SetSubject("Testing the SendGrid C# Library");

            msg.AddContent(MimeType.Text, "Hello World plain text!");
            msg.AddContent(MimeType.Html, "<p>Hello World!</p>");

        


        log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("DurableMailSender_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}