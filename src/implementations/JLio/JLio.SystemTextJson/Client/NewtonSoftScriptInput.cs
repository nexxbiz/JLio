using System.Text.Json;
using Lio.Core.Models;

namespace JLio.SystemTextJson.Client;

public static class SystemTextJsonScriptInput
{
    public static ScriptInput Create(JsonElement data)
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
            Data = JsonDocument.Parse(data)
        };
    }
}