using System;
using JLio.Core.Contracts;
using JLio.Core.Models;

namespace JLio.Core;

public class CommandsProvider : ICommandsProvider, ICommandsProviderRegistrar
{
    private readonly CommandRegistrations registeredCommands = new CommandRegistrations();

    public ICommand this[string command]
    {
        get
        {
            if (!registeredCommands.ContainsKey(command)) return null;
            var commandImplementation = registeredCommands[command];
            return CreateInstance(commandImplementation);
        }
    }

    public ICommandsProviderRegistrar Register<T>() where T : ICommand
    {
        var command = typeof(T);
        var commandInstance = (ICommand) Activator.CreateInstance(command);
        if (commandInstance != null && !registeredCommands.ContainsKey(commandInstance.CommandName))
            registeredCommands.Add(commandInstance.CommandName, new CommandRegistration(command));
        return this;
    }

    private static ICommand CreateInstance(CommandRegistration commandRegistration)
    {
        return (ICommand) Activator.CreateInstance(commandRegistration.Type);
    }
}