using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IItemsFetcher
    {
        string ArrayCloseChar { get; }
        string CurrentItemPathIndicator { get; }
        string PathDelimiter { get; }
        string RootPathIndicator { get; }
        SelectedTokens SelectTokens(string path, JToken data);
        JToken SelectToken(string path, JToken data);
    }
}