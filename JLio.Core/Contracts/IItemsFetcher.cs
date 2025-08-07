using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts;

public interface IItemsFetcher
{
    string ArrayCloseChar { get; }
    string CurrentItemPathIndicator { get; }
    string PathDelimiter { get; }
    string RootPathIndicator { get; }
    string ParentPathIndicator { get; }
    SelectedTokens SelectTokens(string path, JToken data);
    JToken SelectToken(string path, JToken data);
    string GetPath(JToken token);
    JToken NavigateToParent(JToken currentToken, int levels = 1);
    string ResolveRelativePath(string relativePath, JToken currentToken, JToken dataContext);
}