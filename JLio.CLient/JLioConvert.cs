using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;

namespace JLio.Client
{
    public static class JLioConvert
    {
        public static JLioScript Parse(string script)
        {
            return Parse(script, ParseOptions.CreateDefault());
        }

        public static JLioScript Parse(string script, IJLioParseOptions options)
        {
            if (string.IsNullOrEmpty(script)) return new JLioScript();
            var converters = new[] {options.JLioCommandConverter, options.JLioFunctionConverter};
            return JsonConvert.DeserializeObject<JLioScript>(script, converters);
        }

        public static string Serialize(JLioScript script)
        {
            return Serialize(script, ParseOptions.CreateDefault());
        }

        public static string Serialize(JLioScript script, IJLioParseOptions options)
        {
            var converters = new[] {options.JLioCommandConverter, options.JLioFunctionConverter};
            return JsonConvert.SerializeObject(script, Formatting.Indented, converters);
        }
    }
}