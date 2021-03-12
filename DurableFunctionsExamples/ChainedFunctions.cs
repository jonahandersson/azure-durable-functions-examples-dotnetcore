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
using System.Collections.Generic;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace FunctionChainExample
{
    /// <summary>
    ///  Jonah's mini project examples of Azure Durable Functions 
    ///  Pattern: Function Chaining
    /// </summary>
    public static class ChainedFunctions
    {

        private static readonly IConfiguration _configuration;

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
                await context.CallActivityAsync("SendAllGreetingsToEmailActivity", greetingsOutputs);                

                return greetingsOutputs; //Print to console logs 
            }
            catch (Exception)
            {
                //TODO: Error Handling logic here
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
                //var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "\\outputResult.txt"); 
                string outputPath = @"C:\Users\jonah.andersson\Dropbox\Dev_AzureProjects\AzureDurableFunctionsExamplePatterns\DurableFunctionsExamples\outputResult.txt";

                if (outputGreetings.Count > 0)
                {                 

                    // This text is added only once to the file.
                    if (!File.Exists(outputPath))
                    {
                        // Create a file to write to.
                        using (StreamWriter sw = File.CreateText(outputPath))
                        {
                            foreach (var helloNameGreeting in outputGreetings)
                            {
                                sw.WriteLine(helloNameGreeting);
                            }
                          
                        }
                    }

                    using (StreamWriter sw = File.AppendText(outputPath))
                    {
                        foreach (var helloNameGreeting in outputGreetings)
                        {
                            sw.WriteLine(helloNameGreeting);
                        }
                    }


                    //TODO Debug
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
        public static async Task<string> SendEmailsAsync([ActivityTrigger] List<string> messages, ILogger log)
        {
            //TODO 
            var sendGridAPIKey = "<Your SENDGRID API KEY here -->";
            var client = new SendGridClient(sendGridAPIKey);
                
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress("admin@jonahandersson.tech", "SendGrid Test From App"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress("cjonah@example.se", "Jane Doe"),
                new EmailAddress("anna@example.com", "Anna Lidman"),
                new EmailAddress("peter@example.com", "Peter Saddow")
            };
            

            msg.AddTos(recipients);
            msg.SetSubject("Hello To List Of People Sender using SendGrid and Azure Durable Functions");
            msg.AddContent(MimeType.Text, "Hello World plain text!");
            msg.AddContent(MimeType.Html, "<p>Hello World!</p>");

            if (messages.Count > 0)
            {
                foreach (var helloMessage in messages)
                {
                    msg.AddContent(MimeType.Html, "<b>" + helloMessage + "</b>");
                }              
            }
         
          
            var displayRecipients = false; // set this to true if you want recipients to see each others mail id          
            var response = await client.SendEmailAsync(msg);

            bool isEmailSent = false;

            if (response.IsSuccessStatusCode)
            {
                isEmailSent = true;
            }

            //TODO : Add output text as attachment to email 
            //var helloNamesAttachments = new Attachment()
            //{
            //    Content = Convert.ToBase64String(outputResultFilePath);
            //    Type = "text/txt",
            //    Filename = "HelloNamesPoweredByAzureDurableFunctions.png",
            //    Disposition = "inline",
            //    ContentId = "Banner 2"
            //};
            //msg.AddAttachment(banner2);


            log.LogInformation($"sending email to  {recipients }.");
            log.LogInformation($"All emails sent {isEmailSent }.");
            return $"Email sent is {isEmailSent}!";
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