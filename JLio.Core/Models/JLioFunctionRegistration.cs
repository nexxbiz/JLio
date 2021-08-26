using System;

namespace JLio.Core.Models
{
    public class JLioFunctionRegistration
    {
        public JLioFunctionRegistration(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}