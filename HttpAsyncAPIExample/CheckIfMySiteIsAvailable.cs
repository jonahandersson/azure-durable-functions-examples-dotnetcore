using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace HttpAsyncAPIExample
{
    public static class CheckIfMySiteIsAvailable
    {
        [FunctionName("CheckWebsiteIsAvailable")]
        public static async Task CheckWebsiteIsAvailable([OrchestrationTrigger] IDurableOrchestrationContext context, 
            ILogger log)
        {
            try
            {
                Uri jonahsUrl = new Uri("https://jonahandersson.tech");

                // Makes an HTTP GET request to the specified endpoint like a url
                DurableHttpResponse httpResponse = await context.CallHttpAsync(HttpMethod.Get, jonahsUrl);

                if ((int)httpResponse.StatusCode >= 500 && (int)httpResponse.StatusCode < 600)
                    throw new HttpRequestException("Hey, there is a server error! Something wrong with your website. Fix it Jonah!");
                else
                {
                  await context.CallActivityAsync<string>("CheckHttpResponseActivity", httpResponse);
                    log.LogInformation($"Completed: STATUS CODE  = {httpResponse.StatusCode}");
                    log.LogInformation($"Completed: HEADER  = {httpResponse.Headers}");
                }             
        
            }
            catch (Exception ex)
            {
                log.LogError($"Error at CheckIfMySiteIsAvailable(): { ex.Message} { ex.InnerException}");
                throw;
            }                
             
        }


    }
}