using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;

namespace JLio.Client
{
    public static class JLioScript
    {
        public static CommandLines Parse(string script)
        {
            return Parse(script, JLioParseOptions.CreateDefault());
        }

        public static CommandLines Parse(string script, IJLioParseOptions options)
        {
            var converters = new[] {options.JLioCommandConverter, options.JLioFunctionConverter};
            return JsonConvert.DeserializeObject<CommandLines>(script, converters);
        }
    }
}