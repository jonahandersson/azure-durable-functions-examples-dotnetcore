using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctionChainExample
{
    public static class SendAllGreetingsToEmailActivity
    {
        [FunctionName("SendAllGreetingsToEmailActivity")]
        public static async Task SendGreetingsToEmail([ActivityTrigger] string email, ILogger log)
        {
           await SendEmail(email, log);

        }

        private static async Task SendEmail(string email, ILogger log)
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
    }
}
