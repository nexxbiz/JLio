using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core;
using JLio.Core.Extensions;
using JLio.Extensions.ETL.Commands.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JLio.Extensions.ETL.Commands
{
    public class Resolve : CommandBase
    {
        private bool foundErrors = false;
        private IExecutionContext? executionContext;

        [JsonProperty("path")]
        public required string Path { get; set; }

        [JsonProperty("resolveSettings")]
        public List<ResolveSetting> ResolveSettings { get; set; } = new List<ResolveSetting>();

        public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
        {
            executionContext = context;
            foundErrors = false;
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i =>
                    context.LogWarning(CoreConstants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            }

            var targetTokens = context.ItemsFetcher.SelectTokens(Path, dataContext);

            foreach (var targetToken in targetTokens)
            {
                ExecuteResolve(targetToken, dataContext);
            }

            context.LogInfo(CoreConstants.CommandExecution,
                $"{CommandName}: completed for {Path}");

            return new JLioExecutionResult(!foundErrors, dataContext);
        }

        public override ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(Path))
                result.ValidationMessages.Add($"Path property for {CommandName} command is missing");

            if (ResolveSettings == null || !ResolveSettings.Any())
                result.ValidationMessages.Add($"ResolveSettings property for {CommandName} command is missing or empty");
            else
            {
                foreach (var setting in ResolveSettings)
                {
                    if (setting.ResolveKeys == null || !setting.ResolveKeys.Any())
                        result.ValidationMessages.Add($"ResolveKeys are required for each resolve setting");

                    if (string.IsNullOrWhiteSpace(setting.ReferencesCollectionPath))
                        result.ValidationMessages.Add($"ReferencesCollectionPath is required for each resolve setting");

                    if (setting.Values == null || !setting.Values.Any())
                        result.ValidationMessages.Add($"Values are required for each resolve setting");
                }
            }

            return result;
        }

        private void ExecuteResolve(JToken targetToken, JToken dataContext)
        {
            if (executionContext == null) return;
            
            try
            {
                foreach (var resolveSetting in ResolveSettings)
                {
                    ExecuteResolveSetting(targetToken, dataContext, resolveSetting);
                }
            }
            catch (Exception ex)
            {
          
                executionContext.LogError(CoreConstants.CommandExecution,
                    $"Error executing resolve command: {ex.Message}");
            }
        }

        private void ExecuteResolveSetting(JToken targetToken, JToken dataContext, ResolveSetting resolveSetting)
        {
            if (executionContext == null) return;
            
            try
            {
                // Get reference collection
                var referenceTokens = executionContext.ItemsFetcher.SelectTokens(resolveSetting.ReferencesCollectionPath, dataContext);
                if (!referenceTokens.Any())
                {
                    executionContext.LogWarning(CoreConstants.CommandExecution,
                        $"No reference collection found at path: {resolveSetting.ReferencesCollectionPath}");
                    return;
                }

                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Found {referenceTokens.Count()} reference tokens at {resolveSetting.ReferencesCollectionPath}");

                // Find matching references
                var matchingReferences = FindMatchingReferences(targetToken, referenceTokens, resolveSetting.ResolveKeys);

                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Found {matchingReferences.Count} matching references for target");

                // Apply values regardless of whether we have matches - let each ResolveValue handle its own behavior
                ApplyResolvedValues(targetToken, dataContext, matchingReferences, resolveSetting.Values, resolveSetting.ResolveKeys);
            }
            catch (Exception ex)
            {
             
                executionContext.LogWarning(CoreConstants.CommandExecution,
                    $"Error in resolve setting for collection {resolveSetting.ReferencesCollectionPath}: {ex.Message}");
            }
        }

        private List<JToken> FindMatchingReferences(JToken targetToken, IEnumerable<JToken> referenceTokens, List<ResolveKey> resolveKeys)
        {
            var matchingReferences = new List<JToken>();

            foreach (var referenceToken in referenceTokens)
            {
                if (IsMatch(targetToken, referenceToken, resolveKeys))
                {
                    matchingReferences.Add(referenceToken);
                }
            }

            return matchingReferences;
        }

        private bool IsMatch(JToken targetToken, JToken referenceToken, List<ResolveKey> resolveKeys)
        {
            // All resolve keys must match for a successful match
            foreach (var resolveKey in resolveKeys)
            {
                if (!IsKeyMatch(targetToken, referenceToken, resolveKey))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsKeyMatch(JToken targetToken, JToken referenceToken, ResolveKey resolveKey)
        {
            if (executionContext == null) return false;
            
            try
            {
                var sourceValues = GetValues(targetToken, resolveKey.KeyPath);
                var referenceValues = GetValues(referenceToken, resolveKey.ReferenceKeyPath);

                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Comparing source values [{string.Join(", ", sourceValues.Select(v => v.ToString()))}] with reference values [{string.Join(", ", referenceValues.Select(v => v.ToString()))}]");

                // Check if any source value matches any reference value
                var hasMatch = sourceValues.Any(sv => referenceValues.Any(rv => AreValuesEqual(sv, rv)));
                
                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Key match result: {hasMatch}");
                    
                return hasMatch;
            }
            catch (Exception ex)
            {
                executionContext.LogWarning(CoreConstants.CommandExecution,
                    $"Error matching keys {resolveKey.KeyPath} -> {resolveKey.ReferenceKeyPath}: {ex.Message}");
                return false;
            }
        }

        private List<JToken> GetValues(JToken token, string path)
        {
            var values = new List<JToken>();

            if (executionContext == null) return values;

            try
            {
                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Getting values from path '{path}' on token: {token}");

                // Handle @. notation for relative paths
                if (path.StartsWith("@."))
                {
                    var relativePath = path.Substring(2); // Remove "@."
                    
                    // Handle array notation like @.keys[*]
                    if (path.EndsWith("[*]"))
                    {
                        var arrayPath = relativePath.Substring(0, relativePath.Length - 3);
                        executionContext.LogInfo(CoreConstants.CommandExecution,
                            $"Handling relative array path: '{arrayPath}'");
                        
                        var arrayToken = token.SelectToken(arrayPath);
                        
                        executionContext.LogInfo(CoreConstants.CommandExecution,
                            $"Found array token: {arrayToken}, Type: {arrayToken?.Type}");
                        
                        if (arrayToken is JArray array)
                        {
                            values.AddRange(array.Children());
                            executionContext.LogInfo(CoreConstants.CommandExecution,
                                $"Added {array.Children().Count()} children from array");
                        }
                    }
                    else
                    {
                        executionContext.LogInfo(CoreConstants.CommandExecution,
                            $"Handling simple relative path: '{relativePath}'");
                            
                        var value = token.SelectToken(relativePath);
                        
                        if (value != null)
                        {
                            values.Add(value);
                            executionContext.LogInfo(CoreConstants.CommandExecution,
                                $"Found value: {value}");
                        }
                    }
                }
                else
                {
                    // Handle absolute paths with ItemsFetcher
                    executionContext.LogInfo(CoreConstants.CommandExecution,
                        $"Handling absolute path: '{path}'");
                        
                    var tokens = executionContext.ItemsFetcher.SelectTokens(path, token);
                    
                    executionContext.LogInfo(CoreConstants.CommandExecution,
                        $"Found {tokens.Count()} tokens for absolute path");
                        
                    values.AddRange(tokens);
                }
                
                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Final values count: {values.Count}, Values: [{string.Join(", ", values.Select(v => v.ToString()))}]");
            }
            catch (Exception ex)
            {
                executionContext.LogWarning(CoreConstants.CommandExecution,
                    $"Error getting values from path {path}: {ex.Message}");
            }

            return values;
        }

        private bool AreValuesEqual(JToken value1, JToken value2)
        {
            try
            {
                return JToken.DeepEquals(value1, value2);
            }
            catch
            {
                return false;
            }
        }

        private void ApplyResolvedValues(JToken targetToken, JToken dataContext, List<JToken> matchingReferences, List<ResolveValue> values, List<ResolveKey> resolveKeys)
        {
            if (executionContext == null) return;
            
            foreach (var resolveValue in values)
            {
                try
                {
                    ApplyResolveValue(targetToken, dataContext, matchingReferences, resolveValue, resolveKeys);
                }
                catch (Exception ex)
                {
                    foundErrors = true;
                    executionContext.LogWarning(CoreConstants.CommandExecution,
                        $"Error applying resolved value to {resolveValue.TargetPath}: {ex.Message}");
                }
            }
        }

        private void ApplyResolveValue(JToken targetToken, JToken dataContext, List<JToken> matchingReferences, ResolveValue resolveValue, List<ResolveKey> resolveKeys)
        {
            JToken? valueToAssign;
            
            // Use the new AsArray behavior property to determine how to format the result
            switch (resolveValue.ResolveTypeBehavior)
            {
                case ResolveTypeBehavior.AlwaysAsArray:
                    valueToAssign = CreateArrayResult(matchingReferences, dataContext, resolveValue.Value);
                    break;
                    
                case ResolveTypeBehavior.AlwaysAsObject:
                    valueToAssign = CreateObjectResult(matchingReferences, dataContext, resolveValue.Value);
                    break;
                    
                case ResolveTypeBehavior.DependingOnResult:
                default:
                    valueToAssign = CreateDependingOnResultResult(matchingReferences, dataContext, resolveValue.Value, resolveKeys);
                    break;
            }

            // Only apply the value if we have something to assign (null means no assignment for some behaviors)
            if (valueToAssign != null)
            {
                SetValueAtPath(targetToken, resolveValue.TargetPath, valueToAssign);
            }
        }

        private JToken CreateArrayResult(List<JToken> matchingReferences, JToken dataContext, IFunctionSupportedValue value)
        {
            var arrayValues = new JArray();
            foreach (var matchingRef in matchingReferences)
            {
                var resolvedValue = GetResolvedValue(matchingRef, dataContext, value);
                if (resolvedValue != null)
                {
                    arrayValues.Add(resolvedValue);
                }
            }
            return arrayValues;
        }

        private JToken? CreateObjectResult(List<JToken> matchingReferences, JToken dataContext, IFunctionSupportedValue value)
        {
            if (matchingReferences.Count == 0)
            {
                return null; // No matches - don't assign anything
            }
            else if (matchingReferences.Count == 1)
            {
                return GetResolvedValue(matchingReferences[0], dataContext, value);
            }
            else
            {
                // Multiple matches but user wants object - this is an error condition
                if (executionContext != null)
                {
                    executionContext.LogError(CoreConstants.CommandExecution,
                        $"Multiple matches found ({matchingReferences.Count}) but AsArray is set to AlwaysAsObject. This is not supported.");
                }
                throw new InvalidOperationException($"Multiple matches found ({matchingReferences.Count}) but AsArray is set to AlwaysAsObject");
            }
        }

        private JToken? CreateDependingOnResultResult(List<JToken> matchingReferences, JToken dataContext, IFunctionSupportedValue value, List<ResolveKey> resolveKeys)
        {
            // This is the original logic - maintain backward compatibility
            bool shouldReturnArray = ShouldReturnArray(resolveKeys);
            
            if (shouldReturnArray)
            {
                // Always return array for array-based matching scenarios
                return CreateArrayResult(matchingReferences, dataContext, value);
            }
            else if (matchingReferences.Count == 1)
            {
                // Single match for simple key matching
                return GetResolvedValue(matchingReferences[0], dataContext, value);
            }
            else if (matchingReferences.Count > 1)
            {
                // Multiple matches for simple key matching - return array
                return CreateArrayResult(matchingReferences, dataContext, value);
            }
            else
            {
                // No matches - don't assign anything
                return null;
            }
        }

        private bool ShouldReturnArray(List<ResolveKey> resolveKeys)
        {
            // Check if any of the resolve keys involves array-to-array matching
            foreach (var resolveKey in resolveKeys)
            {
                // If both keyPath and referenceKeyPath use [*], it's array-to-array matching
                if (resolveKey.KeyPath.EndsWith("[*]") && resolveKey.ReferenceKeyPath.EndsWith("[*]"))
                {
                    return true;
                }
            }
            return false;
        }

        private JToken? GetResolvedValue(JToken matchingReference, JToken dataContext, IFunctionSupportedValue value)
        {
            if (executionContext == null) return JValue.CreateNull();
            
            try
            {
                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Getting resolved value from matching reference: {matchingReference}");
                    
                var result = value.GetValue(matchingReference, dataContext, executionContext);
                
                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Value.GetValue result: Success={result.Success}, Data={result.Data}");
                    
                if (result.Success)
                {
                    // Use the same pattern as IfElse command
                    var jTokenValue = result.Data.GetJTokenValue();
                    executionContext.LogInfo(CoreConstants.CommandExecution,
                        $"Final JToken value: {jTokenValue}");
                    return jTokenValue;
                }
                return JValue.CreateNull();
            }
            catch (Exception ex)
            {
                executionContext.LogWarning(CoreConstants.CommandExecution,
                    $"Error resolving value: {ex.Message}");
                return JValue.CreateNull();
            }
        }

        private void SetValueAtPath(JToken targetToken, string path, JToken value)
        {
            if (executionContext == null) return;
            
            try
            {
                // Simple implementation for relative paths like @.propertyName
                if (path.StartsWith("@.") && targetToken is JObject targetObject)
                {
                    var propertyName = path.Substring(2);
                    targetObject[propertyName] = value;
                }
                else
                {
                    executionContext.LogWarning(CoreConstants.CommandExecution,
                        $"Unsupported target path format: {path}");
                }
            }
            catch (Exception ex)
            {
                executionContext.LogError(CoreConstants.CommandExecution,
                    $"Error setting value at path {path}: {ex.Message}");
            }
        }
    }
}
