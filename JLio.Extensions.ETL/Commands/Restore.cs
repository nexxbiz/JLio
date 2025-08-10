using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.ETL.Commands.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.ETL.Commands
{
    public class Restore : CommandBase
    {
        [JsonProperty("path")]
        public string Path { get; set; } = "$";

        [JsonProperty("restoreSettings")]
        public RestoreSettings RestoreSettings { get; set; } = new();

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
                    if (token is not JObject flatObject) 
                    {
                        context.LogWarning(CoreConstants.CommandExecution, $"Restore can only be applied to objects, found {token.Type}");
                        continue;
                    }
                    
                    // Get metadata
                    var metadata = GetMetadata(flatObject, dataContext, context);
                    
                    if (metadata == null && RestoreSettings.StrictMode)
                    {
                        context.LogError(CoreConstants.CommandExecution, "No flatten metadata found and strict mode is enabled");
                        return new JLioExecutionResult(false, dataContext);
                    }

                    JToken restored;
                    
                    if (RestoreSettings.UseJsonPathColumn && flatObject.ContainsKey(RestoreSettings.JsonPathColumn))
                    {
                        restored = RestoreUsingJsonPath(flatObject, context);
                    }
                    else if (metadata != null)
                    {
                        restored = RestoreFromFlat(flatObject, metadata, context);
                    }
                    else
                    {
                        // Best effort restore without metadata
                        context.LogInfo(CoreConstants.CommandExecution, "Attempting restore without metadata");
                        restored = RestoreWithoutMetadata(flatObject);
                    }
                    
                    // Replace the flattened object with restored structure
                    ReplaceWithRestored(token, restored, dataContext);

                    // Remove metadata if requested
                    if (RestoreSettings.RemoveMetadata && metadata != null)
                    {
                        RemoveMetadata(dataContext, context);
                    }
                }

                return new JLioExecutionResult(true, dataContext);
            }
            catch (Exception ex)
            {
                context.LogError(CoreConstants.CommandExecution, $"Error in Restore command: {ex.Message}");
                return new JLioExecutionResult(false, dataContext);
            }
        }

        private FlattenMetadata? GetMetadata(JObject flatObject, JToken dataContext, IExecutionContext context)
        {
            try
            {
                JToken? metadataToken = null;
                
                if (string.IsNullOrEmpty(RestoreSettings.MetadataPath))
                {
                    metadataToken = flatObject[RestoreSettings.MetadataKey];
                }
                else
                {
                    var metadataContainer = dataContext.SelectToken(RestoreSettings.MetadataPath);
                    metadataToken = metadataContainer?[RestoreSettings.MetadataKey];
                }
                
                return metadataToken?.ToObject<FlattenMetadata>();
            }
            catch (Exception ex)
            {
                context.LogWarning(CoreConstants.CommandExecution, $"Failed to retrieve metadata: {ex.Message}");
                return null;
            }
        }

        private JToken RestoreUsingJsonPath(JObject flatObject, IExecutionContext context)
        {
            var result = new JObject();
            var processedKeys = new HashSet<string> { RestoreSettings.JsonPathColumn };

            // Group by JSONPath
            var pathGroups = new Dictionary<string, List<KeyValuePair<string, JToken>>>
            ();
            
            foreach (var kvp in flatObject.Properties())
            {
                if (processedKeys.Contains(kvp.Name)) continue;

                var jsonPath = kvp.Value.ToString();
                if (!pathGroups.ContainsKey(jsonPath))
                    pathGroups[jsonPath] = new List<KeyValuePair<string, JToken>>();
                    
                pathGroups[jsonPath].Add(new KeyValuePair<string, JToken>(kvp.Name, kvp.Value));
            }

            // Restore each group
            foreach (var pathGroup in pathGroups)
            {
                var jsonPath = pathGroup.Key;
                var values = pathGroup.Value;
                
                foreach (var value in values)
                {
                    SetValueAtJsonPath(result, jsonPath, value.Value);
                }
            }

            return result;
        }

        private JToken RestoreFromFlat(JObject flatObject, FlattenMetadata metadata, IExecutionContext context)
        {
            var result = new JObject();
            var processedKeys = new HashSet<string> { metadata.MetadataKey ?? RestoreSettings.MetadataKey };

            // Add type indicator keys to processed keys if present
            if (metadata.PreserveTypes && !string.IsNullOrEmpty(metadata.TypeIndicator))
            {
                foreach (var prop in flatObject.Properties())
                {
                    if (prop.Name.EndsWith(metadata.TypeIndicator))
                        processedKeys.Add(prop.Name);
                }
            }

            foreach (var kvp in flatObject.Properties())
            {
                if (processedKeys.Contains(kvp.Name)) continue;
                
                var path = kvp.Name.Split(new[] { metadata.Delimiter ?? "." }, StringSplitOptions.RemoveEmptyEntries);
                SetNestedValue(result, path, kvp.Value, metadata);
            }

            return result;
        }

        private JToken RestoreWithoutMetadata(JObject flatObject)
        {
            var result = new JObject();
            var delimiter = RestoreSettings.Delimiter;

            foreach (var kvp in flatObject.Properties())
            {
                // Skip metadata keys
                if (kvp.Name.StartsWith("_")) continue;
                
                var path = kvp.Name.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                SetNestedValueBasic(result, path, kvp.Value);
            }

            return result;
        }

        private void SetValueAtJsonPath(JObject root, string jsonPath, JToken value)
        {
            // Parse JSONPath and set value
            // This is a simplified implementation - in production you might want to use a JSONPath library
            var normalizedPath = jsonPath.Replace("$.", "").Replace("[", ".").Replace("]", "");
            var segments = normalizedPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            
            var current = root;
            for (int i = 0; i < segments.Length - 1; i++)
            {
                var segment = segments[i];
                
                if (int.TryParse(segment, out int index))
                {
                    // This should be handled as part of array creation in parent
                    continue;
                }
                
                if (!current.ContainsKey(segment))
                {
                    // Check if next segment is numeric (array index)
                    if (i + 1 < segments.Length && int.TryParse(segments[i + 1], out _))
                    {
                        current[segment] = new JArray();
                    }
                    else
                    {
                        current[segment] = new JObject();
                    }
                }
                
                if (current[segment] is JObject obj)
                {
                    current = obj;
                }
            }
            
            current[segments.Last()] = value;
        }

        private void SetNestedValue(JObject root, string[] path, JToken value, FlattenMetadata metadata)
        {
            var current = root;
            
            for (int i = 0; i < path.Length - 1; i++)
            {
                var segment = path[i];
                
                // Check if this should be an array based on metadata
                var currentPath = string.Join(metadata.Delimiter ?? ".", path.Take(i + 1));
                var shouldBeArray = metadata.OriginalStructure?.ContainsKey(currentPath) == true && 
                                   metadata.OriginalStructure[currentPath].StartsWith("array");

                if (shouldBeArray)
                {
                    if (!current.ContainsKey(segment))
                    {
                        current[segment] = new JArray();
                    }
                    
                    var array = (JArray)current[segment];
                    var nextSegment = path[i + 1];
                    
                    if (int.TryParse(nextSegment, out int index))
                    {
                        // Ensure array has enough elements
                        while (array.Count <= index)
                        {
                            array.Add(new JObject());
                        }
                        
                        if (array[index] is JObject arrayObj)
                        {
                            current = arrayObj;
                        }
                        i++; // Skip the index segment
                    }
                }
                else
                {
                    if (!current.ContainsKey(segment))
                    {
                        current[segment] = new JObject();
                    }
                    current = (JObject)current[segment];
                }
            }

            // Set the final value with type conversion if needed
            var finalValue = value;
            if (metadata.PreserveTypes && !string.IsNullOrEmpty(metadata.TypeIndicator))
            {
                var typeKey = $"{string.Join(metadata.Delimiter ?? ".", path)}{metadata.TypeIndicator}";
                // Type conversion logic could be added here based on stored type information
            }

            current[path.Last()] = finalValue;
        }

        private void SetNestedValueBasic(JObject root, string[] path, JToken value)
        {
            var current = root;
            
            for (int i = 0; i < path.Length - 1; i++)
            {
                var segment = path[i];
                
                if (int.TryParse(segment, out int index))
                {
                    // Previous segment should have created an array
                    if (i > 0)
                    {
                        var arrayKey = path[i - 1];
                        if (current[arrayKey] is JArray array)
                        {
                            while (array.Count <= index)
                            {
                                array.Add(new JObject());
                            }
                            current = (JObject)array[index];
                        }
                    }
                }
                else
                {
                    // Check if next segment is numeric
                    if (i + 1 < path.Length && int.TryParse(path[i + 1], out _))
                    {
                        if (!current.ContainsKey(segment))
                        {
                            current[segment] = new JArray();
                        }
                    }
                    else
                    {
                        if (!current.ContainsKey(segment))
                        {
                            current[segment] = new JObject();
                        }
                        current = (JObject)current[segment];
                    }
                }
            }

            current[path.Last()] = value;
        }

        private void ReplaceWithRestored(JToken originalToken, JToken restored, JToken dataContext)
        {
            if (originalToken.Parent is JProperty prop)
            {
                prop.Value = restored;
            }
            else if (originalToken.Parent is JArray arr)
            {
                var index = arr.IndexOf(originalToken);
                arr[index] = restored;
            }
            else if (originalToken == dataContext)
            {
                dataContext.Replace(restored);
            }
        }

        private void RemoveMetadata(JToken dataContext, IExecutionContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(RestoreSettings.MetadataPath))
                {
                    (dataContext as JObject)?[RestoreSettings.MetadataKey]?.Remove();
                }
                else
                {
                    var metadataContainer = dataContext.SelectToken(RestoreSettings.MetadataPath) as JObject;
                    metadataContainer?[RestoreSettings.MetadataKey]?.Remove();
                }
            }
            catch (Exception ex)
            {
                context.LogWarning(CoreConstants.CommandExecution, $"Failed to remove metadata: {ex.Message}");
            }
        }

        public override ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrWhiteSpace(Path))
                result.ValidationMessages.Add("Path property is required for restore command");

            if (RestoreSettings == null)
                result.ValidationMessages.Add("RestoreSettings property is required for restore command");
            else
            {
                if (string.IsNullOrEmpty(RestoreSettings.Delimiter))
                    result.ValidationMessages.Add("Delimiter cannot be empty in RestoreSettings");
                    
                if (RestoreSettings.UseJsonPathColumn && string.IsNullOrEmpty(RestoreSettings.JsonPathColumn))
                    result.ValidationMessages.Add("JsonPathColumn cannot be empty when UseJsonPathColumn is true");
            }

            return result;
        }
    }
}