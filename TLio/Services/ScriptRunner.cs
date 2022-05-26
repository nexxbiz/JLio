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
                CommandExecutionContext commandExecutionContext =
                    new CommandExecutionContext(executionContext, executionContext.Output);
            
                var result = await ExecuteCommand(commandExecutionContext, mediator, command, cancellationToken);
                executionContext.CommandExecutionContexts.Add(commandExecutionContext);
                executionContext.UpdateOutput(result.Data);
            }
        }

        private static async Task<ICommandExecutionResult> ExecuteCommand(CommandExecutionContext commandExecutionContext, 
            IMediator mediator, ICommand command, CancellationToken cancellationToken)
        {
            var executionStatus = command.CanExecute(commandExecutionContext);
            if (!executionStatus.CanExecute)
            {
                // add the needed information in the notification
                await mediator.Publish(new CommandExecutionNotPossible(command, executionStatus.Message), cancellationToken);
                return new FailedCommandExecutionResult(commandExecutionContext.Input, executionStatus.Message);
            }

            await mediator.Publish(new CommandExecuting(commandExecutionContext, command), cancellationToken);

            try
            {
                 var result = command.ExecuteAsync(commandExecutionContext);
                 await mediator.Publish(new CommandExecuted(commandExecutionContext, command), cancellationToken);
                 return result;
            }
            catch (Exception ex)
            {
                await mediator.Publish(new CommandExecutionFailed(ex, commandExecutionContext), cancellationToken);
                return new FailedCommandExecutionResult(commandExecutionContext.Input, ex.Message);
            }
        }

        private ScriptExecutionContext CreateScriptExecutionContext(IServiceProvider serviceProvider, Script script,
            IReadOnlyDictionary<string, object> input)
        {
            return new ScriptExecutionContext(serviceProvider, script, input);
        }
    }
}