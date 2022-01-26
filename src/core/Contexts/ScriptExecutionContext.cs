using System;
using System.Collections.Generic;
using Lio.Core.Contracts;
using Lio.Core.Runner;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Lio.Core.Contexts
{
    public class ScriptExecutionContext
    {
        public ScriptExecutionContext(IServiceProvider serviceProvider, ISpecificMutator mutator,
            object input = default)
        {
            Input = input;
            ScriptExecutionLog = ActivatorUtilities.CreateInstance<ScriptExecutionLog>(serviceProvider);
            Mediator = serviceProvider.GetRequiredService<IMediator>();
            SpecificMutator = mutator;
            ScriptInstance = new ScriptInstance();
        }

        public object? Input { get; }

        public IDictionary<string, object?> JournalData { get; } = new Dictionary<string, object?>();

        public IMediator Mediator { get; }

        public object Output { get; set; } = new();
        public ScriptExecutionLog ScriptExecutionLog { get; }

        public ScriptInstance ScriptInstance { get; }

        public IServiceProvider ServiceProvider { get; }

        public ISpecificMutator SpecificMutator { get; }

        public void AddEntry(string commandName, string message)
        {
            ScriptExecutionLog.AddEntry(commandName, message);
        }

        public void AddEntry(string message)
        {
            ScriptExecutionLog.AddEntry(ScriptInstance.CurrentCommand, message);
        }

        public void Fault(Exception? exception, string message, string? commandName, object? activityInput)
        {
        }

        public void Success(string commandName, Instant createdAt)
        {
            var clock = ServiceProvider.GetRequiredService<IClock>();
            ScriptInstance.Commands.Add(new CommandScriptInstance
            {
                StartedAt = createdAt,
                CommandStatus = CommandStatus.Succeeded,
                FinishedAt = clock.GetCurrentInstant()
            });
        }

        public void Success(string commandName)
        {
            var clock = ServiceProvider.GetRequiredService<IClock>();
            ScriptInstance.Commands.Add(new CommandScriptInstance
            {
                CommandStatus = CommandStatus.Succeeded,
                FinishedAt = clock.GetCurrentInstant()
            });
        }
    }
}