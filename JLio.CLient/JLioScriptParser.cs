using JLio.Core.Contracts;
using JLio.Core.Models;

namespace JLio.Client
{
    public class JLioScriptParser
    {
        private readonly IJLioParseOptions options;

        public JLioScriptParser(IJLioParseOptions options)
        {
            this.options = options;
        }

        public CommandLines Parse(string script)
        {
            return JLioScript.Parse(script, options);
        }
    }
}