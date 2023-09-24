using TLio.Models;

namespace TLio.Contracts
{
    public interface IScriptRunner<T>
    {
        Task<ScriptExecutionResult> RunAsync(Script<T> script, IReadOnlyDictionary<string, object> input, CancellationToken cancellationToken);
    }
}