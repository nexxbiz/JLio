using System;

namespace JLio.Core.Models
{
    public class FunctionRegistration
    {
        public FunctionRegistration(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}