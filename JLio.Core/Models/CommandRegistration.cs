using System;

namespace JLio.Core.Models
{
    public class CommandRegistration
    {
        public CommandRegistration(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}