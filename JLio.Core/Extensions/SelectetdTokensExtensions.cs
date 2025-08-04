using System.Linq;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Extensions;

public static class SelectedTokensExtensions
{
    public static JToken GetJTokenValue(this SelectedTokens source)
    {
        if (source.Count == 0) return JValue.CreateNull();

        if (source.Count == 1) return source.First();

        var result = new JArray();
        source.ForEach(i => result.Add(i));
        return result;
    }
}