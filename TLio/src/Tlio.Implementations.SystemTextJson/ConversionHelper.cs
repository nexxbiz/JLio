using System.Text.Json.Nodes;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.SystemTextJson
{

    internal static class ConversionHelper
    {
        public static TargetTypes GetTargetType(Json.Path.Node? node)
        {
            if (node == null)
                return TargetTypes.Undefined;

            if (node.Value is JsonObject)
                return TargetTypes.Object;

            if (node.Value is JsonArray)
                return TargetTypes.Array;

            if (node.Value is JsonValue value)
            {
                //if (value..IsBoolean)
                //    return TargetTypes.Boolean;

                //if (value.IsString)
                //    return TargetTypes.String;

                //if (value.IsNumber)
                //    return TargetTypes.Number;
            }

            return TargetTypes.Undefined;
        }
    }
}
