using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Extensions
{
    public class JsonPathItemsFetcher : IItemsFetcher

    {
        public string CurrentObjectIndicator { get; } = "@";

        public SelectedTokens SelectTokens(string path, JToken data)
        {
            return new SelectedTokens(data.SelectTokens(path));
        }

        public JToken SelectToken(string path, JToken data)
        {
            return data.SelectToken(path);
        }
    }
}