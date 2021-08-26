using Newtonsoft.Json;

namespace JLio.Core.Contracts
{
    public interface IJLioParseOptions
    {
        JsonConverter JLioCommandConverter { get; set; }
        JsonConverter JLioFunctionConverter { get; set; }
    }
}