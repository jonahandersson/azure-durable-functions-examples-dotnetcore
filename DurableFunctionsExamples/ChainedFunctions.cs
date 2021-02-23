using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionChainExample
{
    public static class ChainedFunctions
    {
        [FunctionName("ChainedFunctions_Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var outputs = new List<string>();            
            var nameLists = ReadInputStringsFromFile();

            if (nameLists.Count > 0)
            {
                foreach (var name in nameLists)
                {
                    outputs.Add(await context.CallActivityAsync<string>("ChainedFunctions_NameGreeterActivity", name));
                }
            }
           
            log.LogInformation($" DONE! Said Hello to {nameLists.Count} people " + "\n");
            //TODO: Add a new custom activity  returns strings greeting "Hello" + each name on on the file list and send to email 
            await context.CallActivityAsync("SendAllGreetingsToEmailActivity", outputs);
          
            return outputs; //Print to console logs 
           
        }

        [FunctionName("ChainedFunctions_NameGreeterActivity")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {          
            log.LogInformation($"<--- MESSAGE FROM ACTIVITY FUNCTION --->  Saying hello to {name}." + "\n");
            return $"Hello {name}!";
        }

        [FunctionName("ChainedFunctions_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ChainedFunctions_Orchestrator", null);            

            log.LogInformation($"Started orchestration with ID = '{instanceId}'." + "\n");          

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        [FunctionName("SendAllGreetingsToEmailActivity")]
        public static async Task SendGreetingsToEmail([ActivityTrigger] string email, ILogger log)
        {
            SendEmail(email, log);

        }

        private static void SendEmail(string email, ILogger log)
        {
            if (email is null)
            {
                throw new ArgumentNullException(nameof(email));
            }
            else
            {
                //TODO Send Email Logic here
            }

            //TODO : Send email using SendGrid API 
            // log.LogInformation(sendEmailSuccess);
        }


        public static List<string> ReadInputStringsFromFile()
        {
            List<string> inputStrings = new List<string>();
            //var inputNamesTextFilePath = Path.Combine(Directory.GetCurrentDirectory(), "\\names.txt");
            using (var streamReader = new StreamReader(@"C:\Users\jonah.andersson\Dropbox\Dev_AzureProjects\AzureDurableFunctionsExamplePatterns\DurableFunctionsExamples\names.txt"))
            {
                while (streamReader.Peek() >= 0)
                    inputStrings.Add(streamReader.ReadLine());
            }
            return inputStrings;
        }
    }
}