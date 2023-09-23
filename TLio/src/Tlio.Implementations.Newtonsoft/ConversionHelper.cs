﻿using Newtonsoft.Json.Linq;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.Newtonsoft
{
    internal static class ConversionHelper
    {
        internal static TargetTypes GetTargetType(JToken? token)
        {
            //TODO: Add all conversions
            switch (token.Type)
            {
                case JTokenType.Object:
                    return TargetTypes.Object;
                case JTokenType.Boolean:
                    return TargetTypes.Boolean;
                case JTokenType.String:
                    return TargetTypes.String;
                default: return TargetTypes.Undefined;
            }
        }
    }
}