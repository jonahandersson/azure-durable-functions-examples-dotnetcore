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
       
        [Fact]
        public async Task<Task> Run_returns_multiple_greetings()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(context => context.CallActivityAsync<string>("NameGreeterActivity", "Mae Hodges"))
                .ReturnsAsync("Hello Mae Hodges!");

       
            //TODO: Expected 
            //Assert.Equal(“Hello Tokyo!”, result[0]);
            //Assert.Equal(“Hello Seattle!”, result[1]);
            //Assert.AreEqual(“Hello London!”, result[2]);


          var result = await ChainedFunctions.RunOrchestrator(contextMock.Object);

            Assert.Equal(3, result.Count);
            Assert.Equal("Hello Tokyo!", result[0]);
            Assert.Equal("Hello Seattle!", result[1]);
            Assert.Equal("Hello London!", result[2]);
            return Task.CompletedTask;
        }
    }
}
