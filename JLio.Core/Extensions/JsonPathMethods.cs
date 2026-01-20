using System.Collections.Generic;
using System.Linq;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Extensions;

public static class JsonPathMethods
{
    public static JsonSplittedPath SplitPath(string path)
    {
        return new JsonSplittedPath(path);
    }

    public static List<string> GetIntellisense(string initial, JToken dataObject, IItemsFetcher itemsFetcher)
    {
        var path = string.IsNullOrWhiteSpace(initial.TrimEnd('.')) ? "$" : initial.TrimEnd('.');
        var items = itemsFetcher.SelectTokens(path, dataObject);
        if (items.Count == 0)
        {
            var index = path.LastIndexOf('.');
            path = path.Substring(0, index);
            if (index >= 0)
                items.AddRange(itemsFetcher.SelectTokens(path, dataObject));
        }

        return FilterStartsWith(initial, GetIntellisense(items, path)).Distinct().ToList();
    }

    private static List<string> FilterStartsWith(string path, List<string> items)
    {
        if (items.Any(i => i.StartsWith(path))) return items.Where(i => i.StartsWith(path)).ToList();
        return items;
    }

    private static List<string> GetIntellisense(SelectedTokens tokens, string path)
    {
        var result = new List<string>();
        foreach (var t in tokens)
        {
            result.AddRange(GetIntellisense(t, path));
        }
        return result;
    }

    private static List<string> GetIntellisense(JToken token, string path)
    {
        var result = new List<string>();
        switch (token)
        {
            case JObject o:
                result.AddRange(GetPropertyPaths(o, path));
                break;
            case JArray a:
                result.Add($"{path}[*]");
                break;
        }

        return result;
    }

    private static List<string> GetPropertyPaths(JObject jObject, string path)
    {
        var result = new List<string>();
        foreach (var p in jObject.Properties())
        {
            result.Add($"{path}.{p.Name}");
        }
        return result;
    }
}