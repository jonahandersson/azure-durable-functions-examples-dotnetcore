using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HumanInteractionApprovalFlow
{
    public static class SMSVerificator
    {
        [FunctionName("SMSVerificator_Orchestrator")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            
            string phoneNumber = context.GetInput<string>();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentNullException(
                    nameof(phoneNumber),
                    "A phone number input is required.");
            }

            int challengeCode = await context.CallActivityAsync<int>(
                "E4_SendSmsChallenge",
                phoneNumber);

            using (var timeoutCts = new CancellationTokenSource())
            {
                // The user has 90 seconds to respond with the code they received in the SMS message.
                DateTime expiration = context.CurrentUtcDateTime.AddSeconds(90);
                Task timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

                bool authorized = false;
                for (int retryCount = 0; retryCount <= 3; retryCount++)
                {
                    Task<int> challengeResponseTask =
                        context.WaitForExternalEvent<int>("SmsChallengeResponse");

                    Task winner = await Task.WhenAny(challengeResponseTask, timeoutTask);
                    if (winner == challengeResponseTask)
                    {
                        // We got back a response! Compare it to the challenge code.
                        if (challengeResponseTask.Result == challengeCode)
                        {
                            authorized = true;
                            break;
                        }
                    }
                    else
                    {
                        // Timeout expired
                        break;
                    }
                }

                if (!timeoutTask.IsCompleted)
                {
                    // All pending timers must be complete or canceled before the function exits.
                    timeoutCts.Cancel();
                }

                return authorized;
            }
          
        }

        [FunctionName("Function1_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("E4_SendSmsChallenge")]
        public static int SendSmsChallenge(
        [ActivityTrigger] string phoneNumber,
        ILogger log,
        [TwilioSms(AccountSidSetting = "TwilioAccountSid", AuthTokenSetting = "TwilioAuthToken", From = "%TwilioPhoneNumber%")]
            out CreateMessageOptions message)
        {
            // Get a random number generator with a random seed (not time-based)
            var rand = new Random(Guid.NewGuid().GetHashCode());
            int challengeCode = rand.Next(10000);

            log.LogInformation($"Sending verification code {challengeCode} to {phoneNumber}.");

            message = new CreateMessageOptions(new PhoneNumber(phoneNumber));
            message.Body = $"Your verification code is {challengeCode:0000}";

            return challengeCode;
        }

        [FunctionName("Function1_HttpStart")]
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