
using TLio.Contracts;
using TLio.Models;

namespace TLio.Extensions
{
    public static class ScriptRunnerExtensions
    {
        public static Script<T> AddCommand<T>(this Script<T> script, ICommand<T> command)
        {
            script.Commands.Add(command);
            return script;
        }
    }
}
