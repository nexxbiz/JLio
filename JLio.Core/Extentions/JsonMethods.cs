using System.Collections.Generic;
using System.Linq;
using JLio.Core.Contracts;
using JLio.Core.Models.Path;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Extentions
{
    public static class JsonMethods
    {
        public static Dictionary<string, JToken> ConvertToDictionary(this JObject data)
        {
            var result = new Dictionary<string, JToken>();
            data.Properties().ToList().ForEach(p => result.Add(p.Name, p.Value));
            return result;
        }

        public static Dictionary<string, JToken> ConvertToDictionary(this JObject data, List<string> outputFilter)
        {
            if (!outputFilter.Any()) return ConvertToDictionary(data);
            var result = new Dictionary<string, JToken>();
            outputFilter.ForEach(f =>
            {
                if (data.Properties().Any(p => p.Name == f))
                    result.Add(f, data[f]);
            });
            return result;
        }

        public static JObject ConvertToDataObject(this Dictionary<string, JToken> data)
        {
            var result = new JObject();
            foreach (var keyValuePair in data) result.Add(keyValuePair.Key, keyValuePair.Value);
            return result;
        }

        public static void CheckOrCreateParentPath(JToken data, JsonSplittedPath path, IItemsFetcher dataFetcher,
            IJLioExecutionLogger logger)
        {
            var items = dataFetcher.SelectTokens(path.SelectionPath.ToPathString(), data);
            items.ForEach(i => CheckOrCreateConstructionPath(i, path.ConstructionPath.ToList(), logger));
        }

        public static bool IsPropertyOfTypeArray(string propertyName, JObject o)
        {
            return o.ContainsKey(propertyName) && o[propertyName]?.Type == JTokenType.Array;
        }

        private static void CheckOrCreateConstructionPath(JToken item,
            IReadOnlyCollection<PathElement> constructionPath,
            IJLioExecutionLogger logger)
        {
            var currentObject = item is JObject curObject ? curObject : null;
            if (currentObject == null) return;

            foreach (var pathElement in constructionPath.Take(constructionPath.Count() - 1))
            {
                if (currentObject != null && currentObject.ContainsKey(pathElement.ElementName))
                {
                    if (currentObject[pathElement.ElementName]?.Type != JTokenType.Object) return;
                }
                else
                {
                    currentObject?.Add(pathElement.ElementName, new JObject());
                    logger.Log(LogLevel.Information, JLioConstants.CommandExecution,
                        $"Property {pathElement.ElementName} added to {currentObject?.Path} as an object");
                }

                currentObject = (JObject) currentObject?[pathElement.ElementName];
            }
        }
    }
}