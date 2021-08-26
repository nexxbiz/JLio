using System.Collections.Generic;
using System.Linq;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Extensions
{
    public static class JsonPathMethods
    {
        public static JsonSplittedPath SplitPath(string path)
        {
            return new JsonSplittedPath(path);
        }

        public static List<string> GetIntellisense(string initalpath, JToken dataObject, IItemsFetcher ItemsFetcher)
        {
            var path = string.IsNullOrWhiteSpace(initalpath.TrimEnd('.')) ? "$" : initalpath.TrimEnd('.');
            var items = ItemsFetcher.SelectTokens(path, dataObject);
            if (items.Count == 0)
            {
                var index = path.LastIndexOf('.');
                path = path.Substring(0, index);
                if (index >= 0)
                    items.AddRange(ItemsFetcher.SelectTokens(path, dataObject));
            }

            return FilterStartsWith(initalpath, GetIntellisense(items, path)).Distinct().ToList();
        }

        private static List<string> FilterStartsWith(string path, List<string> items)
        {
            if (items.Any(i => i.StartsWith(path))) return items.Where(i => i.StartsWith(path)).ToList();
            return items;
        }

        private static List<string> GetIntellisense(SelectedTokens tokens, string path)
        {
            var result = new List<string>();
            tokens.ToList().ForEach(t => result.AddRange(GetIntellisense(t, path)));
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
            jObject.Properties().ToList().ForEach(p => result.Add($"{path}.{p.Name}"));
            return result;
        }
    }
}