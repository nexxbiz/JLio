using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IItemsFetcher
    {
        string CurrentObjectIndicator { get; }
        SelectedTokens SelectTokens(string path, JToken data);
    }
}