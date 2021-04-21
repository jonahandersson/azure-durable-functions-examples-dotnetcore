using FunctionChainExample;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

namespace AzureDurableFunctions.Tests
{
    public class FunctionChainExampleTest
    {
        readonly ILogger logger; 
       
        [Fact]
        public async Task<Task> RunOchestractor_returns_multiple_greetings_fromList()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(context => context.CallActivityAsync<string>("ChainedFunctions_NameGreeterActivity", "Jonah Andersson"))
                .ReturnsAsync("Hello Jonah Andersson! Welcome to DEVSHOW by Sherry and Rasmus! :) !");    

          var result = await ChainedFunctions.RunOrchestrator(contextMock.Object, logger);

            Assert.Equal(100, result.Count);
        

            //Assert.Equal("Hello Tokyo!", result[0]);
            //Assert.Equal("Hello Seattle!", result[1]);
            //Assert.Equal("Hello London!", result[2]);
            return Task.CompletedTask;
        }
    }
}
