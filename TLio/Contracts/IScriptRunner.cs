using TLio.Models;

namespace TLio.Contracts
{
    public interface IScriptRunner
    {
        Task<ScriptExecutionResult> RunAsync(Script script, IReadOnlyDictionary<string, object> input, CancellationToken cancellationToken);
    }
}