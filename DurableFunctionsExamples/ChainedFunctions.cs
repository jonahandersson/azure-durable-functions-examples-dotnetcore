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
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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
                //Lists to save data 
                var greetingsOutputs = new List<string>();
                var exportedGreetingsOutput = new List<string>();            
              
                string pathToInputFile = GetLocalFileInputFileToRead();

                // CHAIN # 1 - Activity async function with retry. Function reads input text from file with retry option
                var nameLists = await context.CallActivityWithRetryAsync<List<Person>>("ChainedFunctions_ReadInputStringsFromFile",
                    new RetryOptions(TimeSpan.FromSeconds(30),3)
                    { Handle = ex => ex.InnerException.Message == "Reading input strings from file failed."}, pathToInputFile);

                // CHAIN #2 - Add names to output list and greet each person in the text file using NameGreetingActivity
                if (nameLists.Count > 0)
                {
                    foreach (var personName in nameLists)
                    {
                        greetingsOutputs.Add(await context.CallActivityAsync<string>("ChainedFunctions_NameGreeterActivity", personName));
                    }
                }              

                //CHAIN#3 - Read each greeting output and save it into a another text file
                if (greetingsOutputs.Count > 0)
                {
                    //  Save greetings result to output text file
                    await context.CallActivityAsync("ChainedFunctions_SaveToOutputResultFileActivity", greetingsOutputs);
                }              

                log.LogInformation($" DONE! Said Hello to {nameLists.Count} people. " + "\n");
                return greetingsOutputs; //Print to console logs 

            }
            catch (Exception ex)
            {
                //TODO: Error Handling logic here   
               // DoYourCleanUp();
                log.LogError($" Error! Something went wrong. Fix it or handle it! {ex.Message} " + "\n");
                throw;
            }           
        }

        private static void DoYourCleanUp()
        {
            //TODO 
            throw new NotImplementedException();
        }

        [FunctionName("ChainedFunctions_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
          [DurableClient] IDurableOrchestrationClient starter,
          ILogger log)
        {

            // Function input comes from the request content. You can pass your data here
            string instanceId = await starter.StartNewAsync("ChainedFunctions_Orchestrator", null);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'." + "\n");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("ChainedFunctions_NameGreeterActivity")]
        public static string SayHello([ActivityTrigger] Person person, ILogger log)
        {
            string greetingMessagePerPerson = $"Hello {person.Name}! Welcome to the DEV Show! :) ";
            log.LogInformation($"<--- MESSAGE FROM ACTIVITY FUNCTION ---> ");
            log.LogInformation(greetingMessagePerPerson);
            return greetingMessagePerPerson;
        }

        [FunctionName("ChainedFunctions_ReadInputStringsFromFile")]
        public static List<Person> ReadInputFromFileAsync([ActivityTrigger] string pathToInputFile, ILogger log)
        {
            try
            {
                var nameList = new List<Person>();
                List<string> inputStrings = new List<string>();
                //var inputNamesTextFilePath = Path.Combine(Directory.GetCurrentDirectory(), "\\names.txt");
                log.LogInformation("Reading strings of name from the input file.");

                using (var streamReader = new StreamReader(pathToInputFile))
                {
                    while (streamReader.Peek() >= 0)
                        inputStrings.Add(streamReader.ReadLine());
                }

                if (inputStrings != null || inputStrings.Count > 0)
                {
                    foreach (var fullName in inputStrings)
                    {
                        Person person = new Person(fullName);
                        nameList.Add(person);
                    }
                }
             
                return nameList;
            }
            catch (Exception ex)
            {
                //TODO: Handle errors                 
                log.LogError($" Error! Something went wrong. Fix it or handle it! {ex.Message} " + "\n");
                return new List<Person>();
            }
        }
        [FunctionName("ChainedFunctions_SaveToOutputResultFileActivity")]
        public static List<string> SaveGreetingsToOutputLocalFile([ActivityTrigger] List<string> outputGreetings, ILogger log)
        {
            try
            {
                // string pathToOutputFileFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\outputGreetings.txt");
                string pathToOutputFileFile = @"C:\Users\jonah.andersson\Dropbox\Dev_AzureProjects\AzureDurableFunctionsExamplePatterns\DurableFunctionsExamples\Data\outputResult.txt";
                if (outputGreetings.Count > 0)
                {
                   
                    // This text is added only once to the file.
                    if (!File.Exists(pathToOutputFileFile))
                    {
                        // Create a file to write to.
                        using (StreamWriter sw = File.CreateText(pathToOutputFileFile))
                        {
                         
                            foreach (var helloNameGreeting in outputGreetings)
                            {
                                sw.WriteLine(helloNameGreeting);
                            }
                          
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.CreateText(pathToOutputFileFile))
                        {
                            foreach (var helloNameGreeting in outputGreetings)
                            {
                                sw.WriteLine(helloNameGreeting);
                            }
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

        [FunctionName("ChainedFunctions_SaveGreetingsToOutputToAzureStorage")]
        public static async Task<string> SaveGreetingsToOutputToAzureStorageAsync([ActivityTrigger] List<string> outputGreetings, ILogger log)
        {
            string connectionString = "";
            string blobContainerName = "jonahsgreetingscontainer";
            string blobName = "helloazdurablefunctionsgreetings.txt";

            try
            {

                await CreateContainerAndUploadBlobAsync(connectionString, blobContainerName, blobName, log);
                var blobItems = await ListContainersWithTheirBlobsAsync(connectionString, log);
                
                //TODO Verify AZ Portal 
                // TODO await DeleteContainerAsync();

            }
            catch (Exception ex)
            {
                //TODO : Handle errors and exceptions

                log.LogError($"Error: {ex.InnerException}");

            }
            return "Test Added to Azure Blob";
        }

        [FunctionName("SendAllGreetingsToEmailActivity")]
        public static async Task<string> SendEmailsAsync([ActivityTrigger] List<string> messages, ILogger log)
        {

            try
            {
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
                        msg.AddContent(MimeType.Html, "<b>" + helloMessage.ToString() + "</b>");
                    }
                }

                var response = await client.SendEmailAsync(msg);
                bool isEmailSent = false;
                if (response.IsSuccessStatusCode)
                {
                    isEmailSent = true;
                }


                log.LogInformation($"sending email to  {recipients }.");
                log.LogInformation($"All emails sent {isEmailSent }.");
                return $"Email sent is {isEmailSent}!";
            }
            catch (Exception)
            {
                //TODO: Handle errors 

                throw;
            }
        }

        public static string GetLocalFileInputFileToRead()
        {
            return @"<<The Path to your local file with data ex. \Data\names.txt";

        }

        #region Private Async Tasks Functions 
        private static async Task<string> CreateContainerAndUploadBlobAsync(string connectionString, string blobContainerName, string blobName,  ILogger log)
        {
            var outputGreetingsUrl = " ";
            try
            {             
                // Create the Blob Container
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                BlobContainerClient blobContainerClient =
                    blobServiceClient.GetBlobContainerClient(blobContainerName);

                log.LogInformation($"Creating blob container '{blobContainerName}'");

                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

                // Upload Blob
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

                log.LogInformation($"Uploading blob '{blobClient.Name}'");
                log.LogInformation($"   > {blobClient.Uri}");

                using FileStream fileStream = File.OpenRead(@"\Data\outputResult.txt");

                await blobClient.UploadAsync(fileStream,
                 new BlobHttpHeaders { ContentType = "text/txt" });

                outputGreetingsUrl =  blobContainerClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                //TODO : Handle errors and exceptions

                log.LogError($"Error: {ex.InnerException}");

            }
            return outputGreetingsUrl;
        }

        private static async Task<List<string>> ListContainersWithTheirBlobsAsync(string connectionString, ILogger log)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            var blobNameItems = new List<string>();

            log.LogInformation("Listing containers and blobs "
                + $"of '{blobServiceClient.AccountName}' account");

            await foreach (BlobContainerItem blobContainerItem  in blobServiceClient.GetBlobContainersAsync())
            {
                log.LogInformation($"   > {blobContainerItem.Name}");

                BlobContainerClient blobContainerClient =
                  blobServiceClient.GetBlobContainerClient(blobContainerItem.Name);

                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                    blobNameItems.Add(blobItem.Name);
                    log.LogInformation($" - {blobItem.Name}");
                }
            }

            return blobNameItems;
        }

        #endregion

      
    }
}