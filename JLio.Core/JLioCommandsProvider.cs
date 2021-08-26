using System;
using JLio.Core.Contracts;
using JLio.Core.Models;

namespace JLio.Core
{
    public class JLioCommandsProvider : IJLioCommandsProvider, IJLioCommandsProviderRegistrar
    {
        private readonly JLioCommandRegistrations registeredCommands = new JLioCommandRegistrations();

        public IJLioCommand this[string command]
        {
            get
            {
                if (!registeredCommands.ContainsKey(command)) return null;
                var commandImplementation = registeredCommands[command];
                return CreateInstance(commandImplementation);
            }
        }

        public IJLioCommandsProviderRegistrar Register<T>() where T : IJLioCommand
        {
            var command = typeof(T);
            var commandInstance = (IJLioCommand) Activator.CreateInstance(command);
            if (commandInstance != null && !registeredCommands.ContainsKey(commandInstance.CommandName))
                registeredCommands.Add(commandInstance.CommandName, new JLioCommandRegistration(command));
            return this;
        }

        private static IJLioCommand CreateInstance(JLioCommandRegistration commandRegistration)
        {
            return (IJLioCommand) Activator.CreateInstance(commandRegistration.Type);
        }
    }
}