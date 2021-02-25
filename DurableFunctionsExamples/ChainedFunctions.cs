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
    /// <summary>
    ///  Jonah's mini project examples of Azure Durable Functions 
    ///  Pattern: Function Chaining
    /// </summary>
    public static class ChainedFunctions
    {
        [FunctionName("ChainedFunctions_Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            try
            {
                var greetingsOutputs = new List<string>();
                var exportedGreetingsOutput = new List<string>();

                // Read names from local text file .txt 
                //TODO: Make this method as activity               
                var nameLists = ReadInputStringsFromFile();

                // CHAIN #1 - Add names to output list and greet each person in the text file using NameGreetingActivity
                if (nameLists.Count > 0)
                {
                    foreach (var name in nameLists)
                    {
                        greetingsOutputs.Add(await context.CallActivityAsync<string>("ChainedFunctions_NameGreeterActivity", name));
                    }
                }

                log.LogInformation($" DONE! Said Hello to {nameLists.Count} people " + "\n");

                //CHAIN#2 - Email the output greetings using SendGrid API
                if (greetingsOutputs.Count > 0)
                {
                    await context.CallActivityAsync("ChainedFunctions_SaveToOutputResultFileActivity", greetingsOutputs);
                }
            
                //CHAIN#3 - Email the output greetings using SendGrid API
                //TODO: Nullcheck & add sendgrid API to send email
                await context.CallActivityAsync("SendAllGreetingsToEmailActivity", exportedGreetingsOutput);                

                return greetingsOutputs; //Print to console logs 
            }
            catch (Exception)
            {

                throw;
            }           
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

        [FunctionName("ChainedFunctions_SaveToOutputResultFileActivity")]
        public static List<string> SaveGreetingsToOutputLocalFile([ActivityTrigger] List<string> outputGreetings, ILogger log)
        {
            try
            {
                if (outputGreetings.Count > 0)
                {
                    File.WriteAllLinesAsync("outputResult.txt", outputGreetings); //TODO Debug
                }
               log.LogInformation($"Done writng greetings to output text file. Input FIle had total names of '{outputGreetings.Count}'." + "\n");

                return outputGreetings;
            }
            catch (Exception)
            {
                //TODO : Handle errors and exceptions 
                throw;
            }
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