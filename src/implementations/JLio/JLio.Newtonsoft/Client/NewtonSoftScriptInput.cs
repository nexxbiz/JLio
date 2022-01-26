using Lio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Newtonsoft.Client;

public static class NewtonSoftScriptInput
{
    public static ScriptInput Create(JToken data)
    {
        return new ScriptInput
        {
            Data = data
        };
    }

    public static ScriptInput Create(string data)
    {
        return new ScriptInput
        {
            Data = JToken.Parse(data)
        };
    }
}