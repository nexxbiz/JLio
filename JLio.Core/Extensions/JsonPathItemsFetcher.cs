using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Extensions;

public class JsonPathItemsFetcher : IItemsFetcher

{
    public string ArrayCloseChar { get; } = "]";
    public string CurrentItemPathIndicator { get; } = "@";

    public string PathDelimiter { get; } = ".";

    public SelectedTokens SelectTokens(string path, JToken data)
    {
        return new SelectedTokens(data.SelectTokens(path));
    }

    public JToken SelectToken(string path, JToken data)
    {
        return data.SelectToken(path);
    }

    public string RootPathIndicator { get; } = "$";
}