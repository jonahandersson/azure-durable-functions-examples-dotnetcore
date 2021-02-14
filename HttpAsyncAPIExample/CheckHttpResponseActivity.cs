using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Linq;

namespace HttpAsyncAPIExample
{
   public static class CheckHttpResponseActivity
    {

        [FunctionName("CheckHttpResponseActivity")]
        public static string CheckHttpResponseMessage([ActivityTrigger] string name, ILogger log)
        {
           
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }
    }
}
