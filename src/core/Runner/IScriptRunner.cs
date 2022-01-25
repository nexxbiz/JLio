using System.Threading;
using System.Threading.Tasks;
using Lio.Core.Models;

namespace Lio.Core.Runner
{
    public interface IScriptRunner
    {
        Task<ScriptExecutionResult> RunScriptAsync(ScriptDefinition scriptDefinition, ScriptInput input = default, CancellationToken cancellationToken = default);
    }
}