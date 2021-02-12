using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureDurableFunctions.Tests
{
    public class FunctionChainExampleTest : FunctionChainExampleTest.DurableFunctionTest
    {
       
        [Fact]
        public async Task Run_Orchectrator()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();
            contextMock.Setup(context => context.CallActivityAsync<string>(“DurableFunctions_Hello”, “Tokyo”)).Returns(Task.FromResult<string>(“Hello Tokyo!”));
            contextMock.Setup(context => context.CallActivityAsync<string>(“DurableFunctions_Hello”, “Seattle”)).Returns(Task.FromResult<string>(“Hello Seattle!”));
            contextMock.Setup(context => context.CallActivityAsync<string>(“DurableFunctions_Hello”, “London”)).Returns(Task.FromResult<string>(“Hello London!”));
            var result = await  .RunOrchestrator(contextMock.Object);
            Assert.Equal(“Hello Tokyo!”, result[0]);
            Assert.Equal(“Hello Seattle!”, result[1]);
            Assert.AreEqual(“Hello London!”, result[2]);
        }
    }
}
