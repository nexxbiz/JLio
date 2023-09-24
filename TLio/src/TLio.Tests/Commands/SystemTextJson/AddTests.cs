using NUnit.Framework;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TLio.Contexts;
using TLio.Implementations.SystemTextJson;


namespace TLio.Tests.Commands.SystemTextJson
{
    internal class AddTests
    {
        private CommandExecutionContext<JsonNode> commandExecutionContext;

        [SetUp]
        public void Setup()
        {
            string jsonString = @"{ ""demo"": {} }";
            JsonNode node = JsonNode.Parse(jsonString);

            commandExecutionContext = new CommandExecutionContext<JsonNode>(
               new SystemTextJsonExecutionContext(),
               node
               ); 
        }

        [Test]
        public async Task AddPropertyToObjectUsingSystemTextJson()
        {
            var command = new Add("$.demo.newItem", JsonValue.Create(5));

            var result =  command.ExecuteAsync(commandExecutionContext);
            
        }
    }
}
