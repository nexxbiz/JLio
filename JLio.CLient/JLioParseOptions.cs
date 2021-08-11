using JLio.Commands;
using JLio.Core;
using JLio.Core.Contracts;
using Newtonsoft.Json;

namespace JLio.Client
{
    public class JLioParseOptions : IJLioParseOptions
    {
        public JsonConverter JLioCommandConverter { get; set; }

        public static JLioParseOptions CreateDefault()
        {
            var commandProvider = new JLioCommandsProvider();
            commandProvider.Register<Add>();

            return new JLioParseOptions
            {
                JLioCommandConverter = new JLioCommandConverter(commandProvider)
            };
        }
    }
}