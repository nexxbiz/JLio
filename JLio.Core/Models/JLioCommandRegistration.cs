using System;

namespace JLio.Core.Models
{
    public class JLioCommandRegistration
    {
        public JLioCommandRegistration(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}