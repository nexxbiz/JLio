using Lio.Core;
using Lio.Core.Contracts;
using Lio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Newtonsoft.Fetcher
{
    public class JsonPathFetcher : ISpecificFetcher
    {
        public FetchedItems GetItemsForPath(string path, object data)
        {
            var jData = data as JToken;
            if (jData == null) return FetchedItems.Failed();

            return FetchedItems.Success(Convert(jData.SelectTokens(path)));
        }

        private List<FetchedItem> Convert(IEnumerable<JToken> tokens)
        {
            var result = new List<FetchedItem>();
            foreach (var token in tokens) result.Add(Convert(token));

            return result;
        }

        private FetchedItem Convert(JToken jToken)
        {
            return new FetchedItem
            {
                Path = jToken.Path,
                Item = jToken,
                ItemType = GetItemType(jToken)
            };
        }

        private TargetTypes GetItemType(JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Array: return TargetTypes.Array;
                case JTokenType.Boolean: return TargetTypes.Boolean;
                case JTokenType.Bytes: return TargetTypes.Bytes;
                case JTokenType.Comment: return TargetTypes.Comment;
                case JTokenType.Constructor: return TargetTypes.Constructor;
                case JTokenType.Date: return TargetTypes.Date;
                case JTokenType.Float: return TargetTypes.Float;
                case JTokenType.Guid: return TargetTypes.Guid;
                case JTokenType.Integer: return TargetTypes.Integer;
                case JTokenType.None: return TargetTypes.None;
                case JTokenType.Null: return TargetTypes.Null;
                case JTokenType.Object: return TargetTypes.Object;
                case JTokenType.Property: return TargetTypes.Property;
                case JTokenType.Raw: return TargetTypes.Raw;
                case JTokenType.String: return TargetTypes.String;
                case JTokenType.TimeSpan: return TargetTypes.TimeSpan;
                case JTokenType.Undefined: return TargetTypes.Undefined;
                case JTokenType.Uri: return TargetTypes.Uri;
            }

            return TargetTypes.Undefined;
        }
    }
}