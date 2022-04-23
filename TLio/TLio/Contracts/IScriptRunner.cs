using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TLio.Contexts;
using TLio.Models;

namespace TLio.Contracts
{
    public interface IScriptRunner
    {
        Task<ScriptExecutionResult> RunAsync(Script script, IReadOnlyDictionary<string, object> input, CancellationToken cancellationToken);
    }
}