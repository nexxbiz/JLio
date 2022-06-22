using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLio.Contracts;
using TLio.Models;

namespace TLio.Extensions
{
    public static class ScriptRunnerExtensions
    {
        public static Script AddCommand(this Script script, ICommand command)
        {
            script.Commands.Add(command);
            return script;
        }
    }
}
