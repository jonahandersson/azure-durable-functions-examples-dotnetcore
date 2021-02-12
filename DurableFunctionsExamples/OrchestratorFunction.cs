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
    public static class OrchestratorFunction
    {
        [FunctionName("OrchestratorFunction")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            var nameLists = ReadNamesFromFile();

            if (nameLists.Count > 0)
            {
                foreach (var name in nameLists)
                {
                    outputs.Add(await context.CallActivityAsync<string>("NameGreeterActivity", name));
                }
            }

            // returns strings greeting "Hello" + each name on on the file list and send to email 
            await context.CallActivityAsync("SendAllGreetingsToEmailActivity", outputs);
          
            return outputs;
        }

        private static List<string> ReadNamesFromFile()
        {             
            List<string> names = new List<string>();
            using (var streamReader = new StreamReader(@"C:\Users\jonah.andersson\Dropbox\Dev_AzureProjects\AzureDurableFunctionsExamplePatterns\DurableFunctionsExamples\names.txt"))
            {
                while (streamReader.Peek() >= 0)
                    names.Add(streamReader.ReadLine());
            }
            return names;
        }
    }
}