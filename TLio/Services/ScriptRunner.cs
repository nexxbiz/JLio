using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TLio.Contexts;
using TLio.Contracts;
using TLio.Models;
using TLio.Notifications;

namespace TLio.Services
{
    public class ScriptRunner : IScriptRunner
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScriptRunner(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<ScriptExecutionResult> RunAsync(Script script, IReadOnlyDictionary<string, object> input,
            CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var executionContext = CreateScriptExecutionContext(scope.ServiceProvider, script, input);

            var scriptExecutionResult = new ScriptExecutionResult
            {
                Output = executionContext.Input
            };
            
            //TODO validate script execution
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(new ScriptExecuting(executionContext), cancellationToken);

            await ExecuteCommands(script, cancellationToken, executionContext, mediator);

            await mediator.Publish(new ScriptExecuted(executionContext), cancellationToken);

            scriptExecutionResult.Output = executionContext.Output;
            scriptExecutionResult.ExecutionLogs = executionContext.ExecutionLog.ToList();
            return scriptExecutionResult;
        }

        private static async Task ExecuteCommands(Script script, CancellationToken cancellationToken,
            ScriptExecutionContext executionContext, IMediator mediator)
        {
            foreach (var command in script.Commands)
            {
                CommandExecutionContext commandExecutionContext = null;
                try
                {
                    commandExecutionContext =
                        new CommandExecutionContext(executionContext, command, executionContext.Output);

                    var executionStatus = command.CanExecute(commandExecutionContext);
                    if (executionStatus.CanExecute)
                    {
                        var exception = new Exception(executionStatus.Message);
                        await mediator.Publish(new CommandExecutionFailed(exception, commandExecutionContext), cancellationToken);

                    }

                    await mediator.Publish(new CommandExecuting(commandExecutionContext, command), cancellationToken);

                    var result = command.ExecuteAsync(commandExecutionContext);
                    commandExecutionContext.ScriptExecutionContext.UpdateOutput(result);

                    await mediator.Publish(new CommandExecuted(commandExecutionContext, command), cancellationToken);
                }
                catch (Exception ex)
                {
                    await mediator.Publish(new CommandExecutionFailed(ex, commandExecutionContext), cancellationToken);
                }
            }
        }

        private ScriptExecutionContext CreateScriptExecutionContext(IServiceProvider serviceProvider, Script script,
            IReadOnlyDictionary<string, object> input)
        {
            return new ScriptExecutionContext(serviceProvider, script, input);
        }
    }
}