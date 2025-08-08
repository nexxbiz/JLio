using System;
using System.Collections.Generic;
using System.Linq;
using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.ETL.Commands.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.ETL.Commands
{
    public class Flatten : CommandBase
    {
        [JsonProperty("path")]
        public string Path { get; set; } = "$";

        [JsonProperty("flattenSettings")]
        public FlattenSettings FlattenSettings { get; set; } = new();

        public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
        {
            try
            {
                var validationResult = ValidateCommandInstance();
                if (!validationResult.IsValid)
                {
                    validationResult.ValidationMessages.ForEach(msg =>
                        context.LogWarning(CoreConstants.CommandExecution, msg));
                    return new JLioExecutionResult(false, dataContext);
                }

                var targetTokens = context.ItemsFetcher.SelectTokens(Path, dataContext);
                
                foreach (var token in targetTokens)
                {
                    var flattened = FlattenToken(token, "", new Dictionary<string, object>(), 0);
                    var metadata = CreateMetadata(token, context);
                    
                    // Add JSONPath column if requested
                    if (FlattenSettings.IncludeJsonPath)
                    {
                        foreach (var kvp in flattened.ToList())
                        {
                            var jsonPath = BuildJsonPathForFlattenedKey(kvp.Key, token);
                            flattened[FlattenSettings.JsonPathColumn + "." + kvp.Key] = jsonPath;
                        }
                    }
                    
                    // Replace the original token with flattened structure
                    ReplaceTokenWithFlattened(token, flattened, dataContext);

                    // Store metadata
                    StoreMetadata(dataContext, metadata, context);
                }

                return new JLioExecutionResult(true, dataContext);
            }
            catch (Exception ex)
            {
                context.LogError(CoreConstants.CommandExecution, $"Error in Flatten command: {ex.Message}");
                return new JLioExecutionResult(false, dataContext);
            }
        }

        private Dictionary<string, object> FlattenToken(JToken token, string prefix, Dictionary<string, object> result, int depth)
        {
            // Check max depth
            if (FlattenSettings.MaxDepth > 0 && depth >= FlattenSettings.MaxDepth)
            {
                result[prefix] = token.ToString();
                return result;
            }

            // Check exclude/include paths
            if (ShouldExcludePath(prefix))
                return result;

            if (!ShouldIncludePath(prefix))
                return result;

            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = (JObject)token;
                    if (obj.Count == 0)
                    {
                        result[prefix] = "";
                        break;
                    }
                    
                    foreach (var property in obj.Properties())
                    {
                        var newKey = string.IsNullOrEmpty(prefix) 
                            ? property.Name 
                            : $"{prefix}{FlattenSettings.Delimiter}{property.Name}";
                        FlattenToken(property.Value, newKey, result, depth + 1);
                    }
                    break;
                    
                case JTokenType.Array:
                    var array = (JArray)token;
                    if (array.Count == 0)
                    {
                        result[prefix] = Array.Empty<object>();
                        break;
                    }
                    
                    for (int i = 0; i < array.Count; i++)
                    {
                        var arrayKey = FlattenSettings.IncludeArrayIndices 
                            ? $"{prefix}{FlattenSettings.ArrayDelimiter}{i}"
                            : $"{prefix}{FlattenSettings.Delimiter}{i}";
                        FlattenToken(array[i], arrayKey, result, depth + 1);
                    }
                    break;
                    
                default:
                    var value = token.Value<object>() ?? "";
                    result[prefix] = value;
                    
                    // Add type information if requested
                    if (FlattenSettings.PreserveTypes)
                    {
                        result[$"{prefix}{FlattenSettings.TypeIndicator}"] = token.Type.ToString();
                    }
                    break;
            }

            return result;
        }

        private bool ShouldExcludePath(string path)
        {
            if (FlattenSettings.ExcludePaths == null || !FlattenSettings.ExcludePaths.Any())
                return false;

            return FlattenSettings.ExcludePaths.Any(excludePath => 
                path.StartsWith(excludePath, StringComparison.OrdinalIgnoreCase));
        }

        private bool ShouldIncludePath(string path)
        {
            if (FlattenSettings.IncludePaths == null || !FlattenSettings.IncludePaths.Any())
                return true;

            return FlattenSettings.IncludePaths.Any(includePath => 
                path.StartsWith(includePath, StringComparison.OrdinalIgnoreCase));
        }

        private string BuildJsonPathForFlattenedKey(string flattenedKey, JToken originalToken)
        {
            // Build JSONPath using basic logic
            var segments = flattenedKey.Split(new[] { FlattenSettings.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
            var pathSegments = new List<string> { "$" };

            foreach (var segment in segments)
            {
                if (int.TryParse(segment, out int index))
                {
                    // This is an array index
                    pathSegments[pathSegments.Count - 1] = $"{pathSegments[pathSegments.Count - 1]}[{index}]";
                }
                else
                {
                    pathSegments.Add(segment);
                }
            }

            return string.Join(".", pathSegments).Replace(".[", "[");
        }

        private void ReplaceTokenWithFlattened(JToken originalToken, Dictionary<string, object> flattened, JToken dataContext)
        {
            var flattenedJObject = JObject.FromObject(flattened);

            if (originalToken.Parent is JProperty prop)
            {
                prop.Value = flattenedJObject;
            }
            else if (originalToken.Parent is JArray arr)
            {
                var index = arr.IndexOf(originalToken);
                arr[index] = flattenedJObject;
            }
            else if (originalToken == dataContext)
            {
                dataContext.Replace(flattenedJObject);
            }
        }

        private FlattenMetadata CreateMetadata(JToken originalToken, IExecutionContext context)
        {
            return new FlattenMetadata
            {
                OriginalStructure = AnalyzeStructure(originalToken, ""),
                Delimiter = FlattenSettings.Delimiter,
                ArrayDelimiter = FlattenSettings.ArrayDelimiter,
                IncludeArrayIndices = FlattenSettings.IncludeArrayIndices,
                PreserveTypes = FlattenSettings.PreserveTypes,
                TypeIndicator = FlattenSettings.TypeIndicator,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                RootPath = GenerateJsonPathForToken(originalToken),
                MetadataKey = FlattenSettings.MetadataKey
            };
        }

        private string GenerateJsonPathForToken(JToken token)
        {
            var pathSegments = new List<string>();
            var current = token;
            
            while (current.Parent != null)
            {
                switch (current.Parent)
                {
                    case JProperty property:
                        pathSegments.Insert(0, property.Name);
                        current = property.Parent;
                        break;
                        
                    case JArray array:
                        var index = array.IndexOf(current);
                        pathSegments.Insert(0, $"[{index}]");
                        current = array.Parent;
                        break;
                        
                    default:
                        current = current.Parent;
                        break;
                }
            }
            
            if (pathSegments.Count == 0)
                return "$";
                
            return "$." + string.Join(".", pathSegments).Replace(".[", "[");
        }

        private Dictionary<string, string> AnalyzeStructure(JToken token, string prefix)
        {
            var structure = new Dictionary<string, string>();
            
            switch (token.Type)
            {
                case JTokenType.Object:
                    if (!string.IsNullOrEmpty(prefix))
                        structure[prefix] = "object";
                        
                    foreach (var property in ((JObject)token).Properties())
                    {
                        var newKey = string.IsNullOrEmpty(prefix) 
                            ? property.Name 
                            : $"{prefix}{FlattenSettings.Delimiter}{property.Name}";
                        var childStructure = AnalyzeStructure(property.Value, newKey);
                        foreach (var kvp in childStructure)
                            structure[kvp.Key] = kvp.Value;
                    }
                    break;
                    
                case JTokenType.Array:
                    var array = (JArray)token;
                    structure[prefix] = $"array[{array.Count}]";
                    
                    for (int i = 0; i < array.Count; i++)
                    {
                        var arrayKey = FlattenSettings.IncludeArrayIndices 
                            ? $"{prefix}{FlattenSettings.ArrayDelimiter}{i}"
                            : $"{prefix}{FlattenSettings.Delimiter}{i}";
                        var childStructure = AnalyzeStructure(array[i], arrayKey);
                        foreach (var kvp in childStructure)
                            structure[kvp.Key] = kvp.Value;
                    }
                    break;
            }

            return structure;
        }

        private void StoreMetadata(JToken dataContext, FlattenMetadata metadata, IExecutionContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(FlattenSettings.MetadataPath))
                    return;

                var metadataTarget = dataContext.SelectToken(FlattenSettings.MetadataPath) as JObject ?? 
                                   (dataContext as JObject);
                                   
                if (metadataTarget != null)
                {
                    metadataTarget[FlattenSettings.MetadataKey] = JObject.FromObject(metadata);
                }
            }
            catch (Exception ex)
            {
                context.LogWarning(CoreConstants.CommandExecution, $"Failed to store flatten metadata: {ex.Message}");
            }
        }

        public override ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrWhiteSpace(Path))
                result.ValidationMessages.Add("Path property is required for flatten command");

            if (FlattenSettings == null)
                result.ValidationMessages.Add("FlattenSettings property is required for flatten command");
            else
            {
                if (string.IsNullOrEmpty(FlattenSettings.Delimiter))
                    result.ValidationMessages.Add("Delimiter cannot be empty in FlattenSettings");
                    
                if (FlattenSettings.MaxDepth == 0)
                    result.ValidationMessages.Add("MaxDepth cannot be 0 in FlattenSettings");
            }

            return result;
        }
    }
}