using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lio.Core.Contexts;
using Lio.Core.Contracts;
using Lio.Core.ExecutionResult;
using Lio.Core.Models;
using Lio.Core.Notificator;
using Lio.Core.Validator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Lio.Core.Runner
{
    public enum ScriptStatus
    {
        Completed,
        CompletedWithErrors,
        Invalid
    }

    public class ScriptRunner : IScriptRunner
    {
        private readonly ISpecificFetcher fetcher;
        private readonly IMediator mediator;
        private readonly ISpecificMutator mutator;
        private readonly IServiceScopeFactory scopeFactory;

        public ScriptRunner(ISpecificMutator mutator, IServiceScopeFactory scopeFactory, IMediator mediator,
            ISpecificFetcher fetcher)
        {
            this.mutator = mutator;
            this.scopeFactory = scopeFactory;
            this.mediator = mediator;
            this.fetcher = fetcher;
        }

        public async Task<ScriptExecutionResult> RunScriptAsync(ScriptDefinition scriptDefinition,
            ScriptInput input, CancellationToken cancellationToken = default)
        {
            var executionScope = scopeFactory.CreateScope();

            var executionContext =
                new ScriptExecutionContext(executionScope.ServiceProvider, mutator, fetcher, input.Data);

            var validateScriptExecution = new ValidateScriptExecution(executionContext, scriptDefinition, input);
            await mediator.Publish(validateScriptExecution, cancellationToken);

            if (!validateScriptExecution.CanExecuteScript)
                return new ScriptExecutionResult(false, new ScriptInstance
                {
                    ScriptStatus = ScriptStatus.Invalid
                });

            await mediator.Publish(new ScriptExecuting(executionContext), cancellationToken);

            var result = await RunScript(executionContext, scriptDefinition, cancellationToken);

            await mediator.Publish(new ScriptExecuted(executionContext), cancellationToken);
            result.ExecutionLog = executionContext.ScriptExecutionLog.GetEntries().ToList();
            executionContext.ScriptExecutionLog.Flush();
            return result;
        }

        private async Task<ScriptExecutionResult> RunScript(ScriptExecutionContext scriptExecutionContext,
            ScriptDefinition scriptDefinition, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var command in scriptDefinition)
                {
                    var commandExecutionContext = new CommandExecutionContext(scriptExecutionContext.Input,
                        scriptExecutionContext, command.Name);

                    await mediator.Publish(new CommandExecuting(commandExecutionContext, command), cancellationToken);

                    var result = await TryExecuteCommandAsync(commandExecutionContext, command, cancellationToken);

                    await mediator.Publish(new CommandExecutionResultExecuted(result, commandExecutionContext),
                        cancellationToken);

                    await mediator.Publish(new CommandExecuted(commandExecutionContext, command), cancellationToken);
                }

                return new ScriptExecutionResult(true, scriptExecutionContext.ScriptInstance);
            }
            catch (Exception ex)
            {
                scriptExecutionContext.Fault(ex, ex.Message, null, null);
                scriptExecutionContext.AddEntry($"Faulted- {ex.Message}");
            }

            return new ScriptExecutionResult(false, scriptExecutionContext.ScriptInstance);
        }

        private async Task<ICommandExecutionResult> TryExecuteCommandAsync(
            CommandExecutionContext commandExecutionContext, ICommand command, CancellationToken cancellationToken)
        {
            try
            {
                return command.Execute(commandExecutionContext);
            }
            catch (Exception ex)
            {
                await mediator.Publish(new CommandExecutionFailed(ex, commandExecutionContext),
                    cancellationToken);
            }

            return null;
        }
    }
}