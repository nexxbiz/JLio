using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

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
            var lines = JsonConvert.DeserializeObject<List<IJLioCommand>>(script, converters);
            return new CommandLines(lines);
        }
    }
}