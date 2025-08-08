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

        // Pre-allocate reusable collections to reduce GC pressure
        private readonly Dictionary<string, object> _reuseableFlattenResult = new Dictionary<string, object>(256);
        private readonly Dictionary<string, string> _reusableStructure = new Dictionary<string, string>(128);
        private readonly List<string> _reusablePathSegments = new List<string>(16);

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
                    _reuseableFlattenResult.Clear(); // Reuse dictionary
                    var flattened = FlattenToken(token, "", _reuseableFlattenResult, 0);
                    var metadata = CreateMetadata(token, context);
                    
                    // Add JSONPath column if requested
                    if (FlattenSettings.IncludeJsonPath)
                    {
                        // Avoid ToList() and use efficient enumeration
                        var jsonPathColumn = FlattenSettings.JsonPathColumn;
                        var keysToProcess = new string[flattened.Count];
                        var valuesBuffer = new object[flattened.Count];
                        int index = 0;
                        
                        foreach (var kvp in flattened)
                        {
                            keysToProcess[index] = kvp.Key;
                            valuesBuffer[index] = kvp.Value;
                            index++;
                        }
                        
                        for (int i = 0; i < index; i++)
                        {
                            var jsonPath = BuildJsonPathForFlattenedKey(keysToProcess[i], token);
                            flattened[$"{jsonPathColumn}.{keysToProcess[i]}"] = jsonPath;
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

            // Check exclude/include paths - optimized with early returns
            if (ShouldExcludePath(prefix) || !ShouldIncludePath(prefix))
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
                    
                    var delimiter = FlattenSettings.Delimiter;
                    var isRootLevel = string.IsNullOrEmpty(prefix);
                    
                    foreach (var property in obj.Properties())
                    {
                        var newKey = isRootLevel 
                            ? property.Name 
                            : string.Concat(prefix, delimiter, property.Name); // Optimized concatenation
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
                    
                    var arrayDelim = FlattenSettings.IncludeArrayIndices 
                        ? FlattenSettings.ArrayDelimiter
                        : FlattenSettings.Delimiter;
                    
                    for (int i = 0; i < array.Count; i++)
                    {
                        var arrayKey = string.Concat(prefix, arrayDelim, i.ToString()); // Optimized concatenation
                        FlattenToken(array[i], arrayKey, result, depth + 1);
                    }
                    break;
                    
                default:
                    var value = token.Value<object>() ?? "";
                    result[prefix] = value;
                    
                    // Add type information if requested
                    if (FlattenSettings.PreserveTypes)
                    {
                        result[string.Concat(prefix, FlattenSettings.TypeIndicator)] = token.Type.ToString();
                    }
                    break;
            }

            return result;
        }

        private bool ShouldExcludePath(string path)
        {
            var excludePaths = FlattenSettings.ExcludePaths;
            if (excludePaths == null || excludePaths.Count == 0)
                return false;

            // Optimized path checking using ReadOnlySpan
            var pathSpan = path.AsSpan();
            foreach (var excludePath in excludePaths)
            {
                if (pathSpan.StartsWith(excludePath.AsSpan(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private bool ShouldIncludePath(string path)
        {
            var includePaths = FlattenSettings.IncludePaths;
            if (includePaths == null || includePaths.Count == 0)
                return true;

            // Optimized path checking using ReadOnlySpan
            var pathSpan = path.AsSpan();
            foreach (var includePath in includePaths)
            {
                if (pathSpan.StartsWith(includePath.AsSpan(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private string BuildJsonPathForFlattenedKey(string flattenedKey, JToken originalToken)
        {
            // Reuse list for path segments
            _reusablePathSegments.Clear();
            _reusablePathSegments.Add("$");
            
            var segments = flattenedKey.Split(new[] { FlattenSettings.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var segment in segments)
            {
                if (int.TryParse(segment, out int index))
                {
                    // This is an array index - modify last segment
                    var lastIndex = _reusablePathSegments.Count - 1;
                    _reusablePathSegments[lastIndex] = $"{_reusablePathSegments[lastIndex]}[{index}]";
                }
                else
                {
                    _reusablePathSegments.Add(segment);
                }
            }

            return string.Join(".", _reusablePathSegments).Replace(".[", "[");
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
            _reusablePathSegments.Clear();
            var current = token;
            
            while (current.Parent != null)
            {
                switch (current.Parent)
                {
                    case JProperty property:
                        _reusablePathSegments.Insert(0, property.Name);
                        current = property.Parent;
                        break;
                        
                    case JArray array:
                        var index = array.IndexOf(current);
                        _reusablePathSegments.Insert(0, $"[{index}]");
                        current = array.Parent;
                        break;
                        
                    default:
                        current = current.Parent;
                        break;
                }
            }
            
            if (_reusablePathSegments.Count == 0)
                return "$";
                
            return "$." + string.Join(".", _reusablePathSegments).Replace(".[", "[");
        }

        private Dictionary<string, string> AnalyzeStructure(JToken token, string prefix)
        {
            // Reuse structure dictionary
            _reusableStructure.Clear();
            AnalyzeStructureRecursive(token, prefix, _reusableStructure);
            
            // Return a copy to avoid issues with reuse
            return new Dictionary<string, string>(_reusableStructure);
        }

        private void AnalyzeStructureRecursive(JToken token, string prefix, Dictionary<string, string> structure)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    if (!string.IsNullOrEmpty(prefix))
                        structure[prefix] = "object";
                        
                    var delimiter = FlattenSettings.Delimiter;
                    var isRootLevel = string.IsNullOrEmpty(prefix);
                    
                    foreach (var property in ((JObject)token).Properties())
                    {
                        var newKey = isRootLevel 
                            ? property.Name 
                            : string.Concat(prefix, delimiter, property.Name);
                        AnalyzeStructureRecursive(property.Value, newKey, structure);
                    }
                    break;
                    
                case JTokenType.Array:
                    var array = (JArray)token;
                    structure[prefix] = $"array[{array.Count}]";
                    
                    var arrayDelim = FlattenSettings.IncludeArrayIndices 
                        ? FlattenSettings.ArrayDelimiter
                        : FlattenSettings.Delimiter;
                    
                    for (int i = 0; i < array.Count; i++)
                    {
                        var arrayKey = string.Concat(prefix, arrayDelim, i.ToString());
                        AnalyzeStructureRecursive(array[i], arrayKey, structure);
                    }
                    break;
            }
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