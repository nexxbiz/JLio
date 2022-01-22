using JLio2.Core.Models;
using JLio2.Core.Notificator;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JLio2.Core;

public class ScriptRunner
{
    private readonly IMediator mediator;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly CancellationToken cancellationToken;

    public ScriptRunner(IMediator mediator, IServiceScopeFactory serviceScopeFactory, CancellationToken cancellationToken = default)
    {
        this.mediator = mediator;
        this.serviceScopeFactory = serviceScopeFactory;
        this.cancellationToken = cancellationToken;
    }
    
    public async Task<ScriptExecutionResult> RunScriptAsync(ScriptDefinition scriptDefinition, ScriptInput input)
    {
        var executionScope = serviceScopeFactory.CreateScope();
        var executionContext = new ExecutionContext(executionScope.ServiceProvider, input.Input);

        var validateScriptExecution = new ValidateScriptExecution(executionContext, scriptDefinition, input);
        await mediator.Publish(validateScriptExecution, cancellationToken);
        
        if (!validateScriptExecution.CanExecuteScript)
        {
            return new ScriptExecutionResult(false);
        }
        
        await mediator.Publish(new ScriptExecuting(executionContext), cancellationToken);

        ScriptExecutionResult scriptExecutionResult = null;
        await RunScript(executionContext, scriptDefinition);
        
        return scriptExecutionResult;
    }

    private async Task RunScript(ExecutionContext executionContext, ScriptDefinition scriptDefinition)
    {
        foreach (var command in scriptDefinition)
        {
            await mediator.Publish(new CommandExecuting(executionContext, command), cancellationToken);
            var result = await TryExecuteCommandAsync(executionContext, command);

            await mediator.Publish(new CommandExecuted(executionContext, command), cancellationToken);
        }
    }

    private async Task<IExecutionResult> TryExecuteCommandAsync(ExecutionContext executionContext, ICommand command)
    {
        try
        {
            return command.Execute(executionContext);
        }
        catch(Exception ex)
        {
            await mediator.Publish(new CommandExecutionFailed(ex, executionContext, command.Name), cancellationToken);
            throw;
        }
    }
}