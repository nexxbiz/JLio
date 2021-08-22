using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JLio.Client
{
    public static class JLioConvert
    {
        public static JLioScript Parse(string script)
        {
            return Parse(script, JLioParseOptions.CreateDefault());
        }

        public static JLioScript Parse(string script, IJLioParseOptions options)
        {
            if (string.IsNullOrEmpty(script)){ return new JLioScript(); }
            var converters = new[] { options.JLioCommandConverter, options.JLioFunctionConverter };
            return JsonConvert.DeserializeObject<JLioScript>(script, converters);
        }
    }
}