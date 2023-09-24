using System.Text.Json.Nodes;
using TLio.Contracts;
using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;

namespace TLio.Implementations.SystemTextJson
{
    public class SystemTextJsonExecutionContext : ILibraryExecutionContext<JsonNode>
    {
        public IDataFetcher<JsonNode> DataFetcher => new DataFetcher();

        public IMutator<JsonNode> Mutator => new Mutator();

        public ICollection<ScriptExecutionLog> ExecutionLog => new List<ScriptExecutionLog>();

        public void WriteLog(ScriptExecutionLog scriptExecutionLog)
        {
            throw new NotImplementedException();
        }
    }
}
