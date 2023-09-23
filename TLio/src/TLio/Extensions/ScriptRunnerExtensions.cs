
using TLio.Models;

namespace TLio.Extensions
{
    public static class ScriptRunnerExtensions
    {
        public static Script AddCommand(this Script script, Command command)
        {
            script.Commands.Add(command);
            return script;
        }
    }
}
