using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TLio.Contexts;
using TLio.Implementations.SystemTextJson;


namespace TLio.Tests.Commands.SystemTextJson
{
    internal class AddTests
    {
        private CommandExecutionContext<JsonElement> commandExecutionContext;

        [SetUp]
        public void Setup()
        {
            commandExecutionContext = new CommandExecutionContext<JsonElement>(
               new SystemTextJsonExecutionContext(),
               new JsonElement()
               ); 
        }

        [Test]
        public async Task AddPropertyToObjectUsingSystemTextJson()
        {
            var command = new Add("$.demo", JsonValue.Create(5));

            command.ExecuteAsync(commandExecutionContext);
            
        }
    }
}
