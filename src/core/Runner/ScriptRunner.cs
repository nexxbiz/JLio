using System;
using System.Threading;
using System.Threading.Tasks;
using Lio.Core.Contexts;
using Lio.Core.Contracts;
using Lio.Core.ExecutionResult;
using Lio.Core.Models;
using Lio.Core.Notificator;
using Lio.Core.Runner.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Lio.Core.Runner
{
    public interface IScriptLaunchpad
    {
        
    }

    public enum ScriptStatus
    {
        Completed,
        CompletedWithErrors, 
        Invalid
    }

    public class LioOptions
    {
        public ISpecificMutator Mutator { get; internal set; }
        public IMediator Mediator { get; internal set; }
        public IServiceScopeFactory ServiceScopeFactory { get; internal set; }
    }

    public static class LioOptionsExtensions
    {
        public static LioOptions WithMutator(this LioOptions options, ISpecificMutator mutator)
        {
            options.Mutator = mutator;
            return options;
        }
    }

    public class ScriptRunner : IScriptRunner
    {
        private readonly ISpecificMutator _mutator;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMediator _mediator;

        // private readonly IMediator mediator;
        // private readonly IServiceScopeFactory serviceScopeFactory;
        //
        // public ScriptRunner(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
        // {
        //     this.mediator = mediator;
        //     this.serviceScopeFactory = serviceScopeFactory;
        // }

        public ScriptRunner(ISpecificMutator mutator,IServiceScopeFactory scopeFactory, IMediator mediator)
        {
            _mutator = mutator;
            _scopeFactory = scopeFactory;
            _mediator = mediator;
        }

        public virtual async Task<ScriptExecutionResult> RunScriptAsync(ScriptDefinition scriptDefinition, ScriptInput input, CancellationToken cancellationToken = default)
        {
            var executionScope = _scopeFactory.CreateScope();

            var executionContext = new ScriptExecutionContext(executionScope.ServiceProvider, _mutator, input.Input);

            var validateScriptExecution = new ValidateScriptExecution(executionContext, scriptDefinition, input);
            await _mediator.Publish(validateScriptExecution, cancellationToken);

            if (!validateScriptExecution.CanExecuteScript)
            {
                return new ScriptExecutionResult(false, new ScriptInstance()
                {
                    ScriptStatus = ScriptStatus.Invalid
                });
            }

            await _mediator.Publish(new ScriptExecuting(executionContext), cancellationToken);

            var result = await RunScript(executionContext, scriptDefinition, cancellationToken);
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
                    
                    await _mediator.Publish(new CommandExecuting(commandExecutionContext, command), cancellationToken);

                    var result = await TryExecuteCommandAsync(commandExecutionContext, command, cancellationToken);

                    await _mediator.Publish(new CommandExecutionResultExecuted(result, commandExecutionContext),
                        cancellationToken);

                    await _mediator.Publish(new CommandExecuted(scriptExecutionContext, command), cancellationToken);
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

        private async Task<ICommandExecutionResult> TryExecuteCommandAsync(CommandExecutionContext commandExecutionContext, ICommand command, CancellationToken cancellationToken)
        {
            try
            {
                return command.Execute(commandExecutionContext);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new CommandExecutionFailed(ex, commandExecutionContext),
                    cancellationToken);
            }
            return null;
        }
    }
}