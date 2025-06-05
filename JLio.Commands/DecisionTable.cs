using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JLio.Commands;

public class DecisionTable : CommandBase
{
    private IExecutionContext executionContext;

    public DecisionTable()
    {
    }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("decisionTable")]
    public DecisionTableConfig DecisionTableConfig { get; set; }

    public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
    {
        executionContext = context;
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
            ExecuteDecisionTable(targetToken, dataContext);
        }

        context.LogInfo(CoreConstants.CommandExecution,
            $"{CommandName}: completed for {Path}");

        return new JLioExecutionResult(true, dataContext);
    }

    public override ValidationResult ValidateCommandInstance()
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(Path))
            result.ValidationMessages.Add($"Path property for {CommandName} command is missing");

        if (DecisionTableConfig == null)
            result.ValidationMessages.Add($"DecisionTable property for {CommandName} command is missing");
        else
        {
            if (DecisionTableConfig.Inputs == null || !DecisionTableConfig.Inputs.Any())
                result.ValidationMessages.Add($"DecisionTable inputs are required");

            if (DecisionTableConfig.Outputs == null || !DecisionTableConfig.Outputs.Any())
                result.ValidationMessages.Add($"DecisionTable outputs are required");

            if (DecisionTableConfig.Rules == null || !DecisionTableConfig.Rules.Any())
                result.ValidationMessages.Add($"DecisionTable rules are required");
        }

        return result;
    }

    private void ExecuteDecisionTable(JToken targetToken, JToken dataContext)
    {
        try
        {
            // Evaluate input values
            var inputValues = EvaluateInputs(targetToken, dataContext);

            // Find matching rules
            var matchingRules = FindMatchingRules(inputValues);

            if (matchingRules.Any())
            {
                // Apply results based on execution strategy
                var strategy = DecisionTableConfig.ExecutionStrategy ?? new ExecutionStrategy
                {
                    Mode = "firstMatch",
                    ConflictResolution = "priority",
                    StopOnError = false
                };

                if (strategy.ConflictResolution == "merge" && matchingRules.Count > 1)
                {
                    ApplyMergedResults(matchingRules, targetToken, dataContext);
                }
                else if (strategy.ConflictResolution == "lastWins")
                {
                    ApplyRuleResults(matchingRules.Last(), targetToken, dataContext);
                }
                else
                {
                    // Priority or default - rules already sorted by priority
                    foreach (var rule in matchingRules)
                    {
                        ApplyRuleResults(rule, targetToken, dataContext);
                        if (strategy.Mode == "firstMatch") break;
                    }
                }
            }
            else if (DecisionTableConfig.DefaultResults != null)
            {
                // Apply default results
                ApplyDefaultResults(targetToken, dataContext);
            }

            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"DecisionTable executed for path: {targetToken.Path}");
        }
        catch (Exception ex)
        {
            executionContext.LogError(CoreConstants.CommandExecution,
                $"Error executing DecisionTable for {targetToken.Path}: {ex.Message}");

            if (DecisionTableConfig.ExecutionStrategy?.StopOnError == true)
            {
                throw; // Re-throw to stop execution
            }
        }
    }

    private void ApplyMergedResults(List<DecisionRule> rules, JToken targetToken, JToken dataContext)
    {
        var mergedResults = new Dictionary<string, JToken>();

        // Collect all results from all matching rules
        foreach (var rule in rules)
        {
            foreach (var result in rule.Results)
            {
                var valueToken = EvaluateResultValue(result.Value, targetToken, dataContext);
                if (!mergedResults.ContainsKey(result.Key))
                {
                    mergedResults[result.Key] = valueToken;
                }
                else
                {
                    // Handle merge conflicts
                    mergedResults[result.Key] = MergeValues(mergedResults[result.Key], valueToken);
                }
            }
        }

        // Apply merged results
        foreach (var result in mergedResults)
        {
            var output = DecisionTableConfig.Outputs.FirstOrDefault(o => o.Name == result.Key);
            if (output != null)
            {
                ApplyOutput(output, result.Value, targetToken, dataContext);
            }
        }
    }

    private JToken MergeValues(JToken existing, JToken newValue)
    {
        // If both are arrays, concatenate them
        if (existing is JArray existingArray && newValue is JArray newArray)
        {
            var merged = new JArray(existingArray);
            foreach (var item in newArray)
            {
                merged.Add(item);
            }
            return merged;
        }

        // If one is array and other isn't, convert to array
        if (existing is JArray existingArr)
        {
            existingArr.Add(newValue);
            return existingArr;
        }

        if (newValue is JArray newArr)
        {
            var merged = new JArray { existing };
            foreach (var item in newArr)
            {
                merged.Add(item);
            }
            return merged;
        }

        // For numbers, take the maximum value
        if (decimal.TryParse(existing?.ToString(), out var existingDecimal) &&
            decimal.TryParse(newValue?.ToString(), out var newDecimal))
        {
            return new JValue(Math.Max(existingDecimal, newDecimal));
        }

        // For other types, create an array
        return new JArray { existing, newValue };
    }

    private JToken EvaluateResultValue(JToken resultValue, JToken currentToken, JToken dataContext)
    {
        var functionValue = new FunctionSupportedValue(new FixedValue(resultValue));
        var functionResult = functionValue.GetValue(currentToken, dataContext, executionContext);
        return functionResult.Data.GetJTokenValue();
    }

    private Dictionary<string, object> EvaluateInputs(JToken targetToken, JToken dataContext)
    {
        var inputValues = new Dictionary<string, object>();

        foreach (var input in DecisionTableConfig.Inputs)
        {
            try
            {
                var inputValue = EvaluateInputValue(input, targetToken, dataContext);
                inputValues[input.Name] = inputValue;

                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Input '{input.Name}' evaluated to: {inputValue??"null"}");
            }
            catch (Exception ex)
            {
                executionContext.LogWarning(CoreConstants.CommandExecution,
                    $"Failed to evaluate input '{input.Name}': {ex.Message}");
                inputValues[input.Name] = null;
            }
        }

        return inputValues;
    }

    private object EvaluateInputValue(DecisionInput input, JToken targetToken, JToken dataContext)
    {
        JToken selectedToken;

        // Handle relative vs absolute paths
        if (input.Path.StartsWith("@."))
        {
            // Relative to current target token
            var relativePath = input.Path.Substring(1); // Remove '@'
            selectedToken = targetToken.SelectToken(relativePath);
        }
        else
        {
            // Absolute path from root
            selectedToken = executionContext.ItemsFetcher.SelectToken(input.Path, dataContext);
        }

        if (selectedToken == null)
            return null;

        // Apply transform if specified
        if (!string.IsNullOrEmpty(input.Transform))
        {
            if (input.Transform.StartsWith("="))
            {
                // This is a function - would need to integrate with your function evaluator
                // For now, return the raw value
                return selectedToken.ToObject<object>();
            }
        }

        // Convert to appropriate type
        return ConvertToType(selectedToken, input.Type);
    }

    private object ConvertToType(JToken token, string type)
    {
        if (token == null) return null;

        return type?.ToLower() switch
        {
            "string" => token.ToString(),
            "number" => token.ToObject<decimal?>(),
            "boolean" => token.ToObject<bool?>(),
            "array" => token.ToObject<object[]>(),
            "object" => token.ToObject<object>(),
            _ => token.ToObject<object>()
        };
    }

    private List<DecisionRule> FindMatchingRules(Dictionary<string, object> inputValues)
    {
        var strategy = DecisionTableConfig.ExecutionStrategy ?? new ExecutionStrategy
        {
            Mode = "firstMatch",
            ConflictResolution = "priority",
            StopOnError = false
        };

        var allRules = DecisionTableConfig.Rules;

        // Only sort for firstMatch with priority resolution
        if (strategy.Mode == "firstMatch" && strategy.ConflictResolution == "priority")
        {
            allRules = allRules
                .OrderBy(r => r.Priority)
                .ThenBy(r => DecisionTableConfig.Rules.IndexOf(r))
                .ToList();
        }
        // For bestMatch and allMatches, use document order to find ALL matches first

        var matchingRules = new List<DecisionRule>();

        foreach (var rule in allRules)
        {
            if (EvaluateRuleConditions(rule, inputValues))
            {
                matchingRules.Add(rule);

                if (strategy.Mode == "firstMatch")
                    break; // Only break for firstMatch
            }
        }

        // For bestMatch, find the highest scoring rule among ALL matches
        if (strategy.Mode == "bestMatch" && matchingRules.Any())
        {
            var bestRule = FindBestMatchingRule(matchingRules, inputValues);
            return new List<DecisionRule> { bestRule };
        }

        return matchingRules;
    }
    private DecisionRule FindBestMatchingRule(List<DecisionRule> matchingRules, Dictionary<string, object> inputValues)
    {
        var bestRule = matchingRules.First();
        var bestScore = CalculateRuleScore(bestRule, inputValues);

        foreach (var rule in matchingRules.Skip(1))
        {
            var score = CalculateRuleScore(rule, inputValues);
            if (score > bestScore)
            {
                bestScore = score;
                bestRule = rule;
            }
        }

        executionContext.LogInfo(CoreConstants.CommandExecution,
            $"Best matching rule selected with score: {bestScore} (Priority: {bestRule.Priority})");

        return bestRule;
    }

    private int CalculateRuleScore(DecisionRule rule, Dictionary<string, object> inputValues)
    {
        // Score based on:
        // 1. Number of conditions matched (specificity)
        // 2. Priority value
        var conditionsMatched = rule.Conditions.Count(c => inputValues.ContainsKey(c.Key));
        return (conditionsMatched * 100) + rule.Priority;
    }

    private bool EvaluateRuleConditions(DecisionRule rule, Dictionary<string, object> inputValues)
    {
        foreach (var condition in rule.Conditions)
        {
            if (!inputValues.ContainsKey(condition.Key))
            {
                executionContext.LogWarning(CoreConstants.CommandExecution,
                    $"Input '{condition.Key}' not found for condition evaluation");
                return false;
            }

            var inputValue = inputValues[condition.Key];
            var conditionValue = condition.Value;

            if (!EvaluateCondition(inputValue, conditionValue))
                return false;
        }

        return true;
    }



    private bool EvaluateCondition(object inputValue, JToken conditionValue)
    {
        if (conditionValue.Type == JTokenType.Array)
        {
            // Array membership check - conditionValue is already JArray
            return conditionValue.Any(item => AreEqual(inputValue, item.ToObject<object>()));
        }

        if (conditionValue.Type == JTokenType.String)
        {
            var stringCondition = conditionValue.ToString();
            return EvaluateComplexCondition(inputValue, stringCondition);
        }

        // Direct equality check
        return AreEqual(inputValue, conditionValue.ToObject<object>());
    }

    private bool EvaluateComplexCondition(object inputValue, string condition)
    {
        // Handle OR operations (lowest precedence)
        if (condition.Contains("||"))
        {
            var orParts = SplitRespectingPrecedence(condition, "||");
            return orParts.Any(part => EvaluateAndExpression(inputValue, part.Trim()));
        }

        // No OR operations, evaluate as AND expression
        return EvaluateAndExpression(inputValue, condition);
    }

    private bool EvaluateAndExpression(object inputValue, string expression)
    {
        // Handle AND operations (higher precedence than OR)
        if (expression.Contains("&&"))
        {
            var andParts = SplitRespectingPrecedence(expression, "&&");
            return andParts.All(part => EvaluateSingleCondition(inputValue, part.Trim()));
        }

        // No AND operations, evaluate single condition
        return EvaluateSingleCondition(inputValue, expression);
    }

    private List<string> SplitRespectingPrecedence(string expression, string operatorToSplit)
    {
        var parts = new List<string>();
        var currentPart = "";
        var i = 0;

        while (i < expression.Length)
        {
            // Check if we found the operator to split on
            if (i <= expression.Length - operatorToSplit.Length &&
                expression.Substring(i, operatorToSplit.Length) == operatorToSplit)
            {
                // Add current part and start new one
                parts.Add(currentPart);
                currentPart = "";
                i += operatorToSplit.Length;
            }
            else
            {
                currentPart += expression[i];
                i++;
            }
        }

        // Add the final part
        if (!string.IsNullOrEmpty(currentPart))
            parts.Add(currentPart);

        return parts;
    }

    private bool EvaluateSingleCondition(object inputValue, string condition)
    {
        // Handle comparison operators (order matters - check >= before >)
        if (condition.Contains(">="))
            return CompareValues(inputValue, condition.Replace(">=", "").Trim(), ">=");
        if (condition.Contains("<="))
            return CompareValues(inputValue, condition.Replace("<=", "").Trim(), "<=");
        if (condition.Contains("!="))
            return !AreEqual(inputValue, condition.Replace("!=", "").Trim());
        if (condition.Contains(">"))
            return CompareValues(inputValue, condition.Replace(">", "").Trim(), ">");
        if (condition.Contains("<"))
            return CompareValues(inputValue, condition.Replace("<", "").Trim(), "<");
        if (condition.StartsWith("="))
            return AreEqual(inputValue, condition.Substring(1).Trim());

        // Direct equality check
        return AreEqual(inputValue, condition);
    }



    private bool CompareValues(object inputValue, string conditionValue, string op)
    {
        try
        {
            var inputDecimal = ParseDecimalResilient(inputValue);
            var conditionDecimal = ParseDecimalResilient(conditionValue);

            if (inputDecimal.HasValue && conditionDecimal.HasValue)
            {
                return op switch
                {
                    ">=" => inputDecimal.Value >= conditionDecimal.Value,
                    "<=" => inputDecimal.Value <= conditionDecimal.Value,
                    ">" => inputDecimal.Value > conditionDecimal.Value,
                    "<" => inputDecimal.Value < conditionDecimal.Value,
                    _ => false
                };
            }

            // Fall back to string comparison if either value can't be parsed as decimal
            var stringComparison = string.Compare(inputValue?.ToString(), conditionValue, StringComparison.OrdinalIgnoreCase);
            return op switch
            {
                ">=" => stringComparison >= 0,
                "<=" => stringComparison <= 0,
                ">" => stringComparison > 0,
                "<" => stringComparison < 0,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private decimal? ParseDecimalResilient(object value)
    {
        if (value == null)
            return null;

        // If it's already a numeric type, convert directly
        if (value is decimal d) return d;
        if (value is double db) return (decimal)db;
        if (value is float f) return (decimal)f;
        if (value is int i) return i;
        if (value is long l) return l;

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
            return null;

        // Try multiple parsing strategies
        return TryParseWithInvariantCulture(stringValue) ??
               TryParseWithCurrentCulture(stringValue) ??
               TryParseWithNormalizedString(stringValue) ??
               TryParseWithBothSeparators(stringValue);
    }

    private decimal? TryParseWithInvariantCulture(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }

    private decimal? TryParseWithCurrentCulture(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var result))
            return result;
        return null;
    }

    private decimal? TryParseWithNormalizedString(string value)
    {
        // Normalize the string by replacing culture-specific decimal separator with invariant (dot)
        var currentDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var normalizedValue = value.Replace(currentDecimalSeparator, ".");

        if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }

    private decimal? TryParseWithBothSeparators(string value)
    {
        // Try treating comma as decimal separator
        var withCommaAsDecimal = value.Replace(".", ",");
        if (decimal.TryParse(withCommaAsDecimal, NumberStyles.Number, new CultureInfo("de-DE"), out var result1))
            return result1;

        // Try treating dot as decimal separator  
        var withDotAsDecimal = value.Replace(",", ".");
        if (decimal.TryParse(withDotAsDecimal, NumberStyles.Number, CultureInfo.InvariantCulture, out var result2))
            return result2;

        return null;
    }

    private bool AreEqual(object inputValue, object conditionValue)
    {
        if (inputValue == null && conditionValue == null) return true;
        if (inputValue == null || conditionValue == null) return false;

        return inputValue.ToString().Equals(conditionValue.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyRuleResults(DecisionRule rule, JToken targetToken, JToken dataContext)
    {
        foreach (var result in rule.Results)
        {
            var output = DecisionTableConfig.Outputs.FirstOrDefault(o => o.Name == result.Key);
            if (output != null)
            {
                var valueToken = EvaluateResultValue(result.Value, targetToken, dataContext);
                ApplyOutput(output, valueToken, targetToken, dataContext);
            }
        }
    }

    private void ApplyDefaultResults(JToken targetToken, JToken dataContext)
    {
        foreach (var result in DecisionTableConfig.DefaultResults)
        {
            var output = DecisionTableConfig.Outputs.FirstOrDefault(o => o.Name == result.Key);
            if (output != null)
            {
                var valueToken = EvaluateResultValue(result.Value, targetToken, dataContext);
                ApplyOutput(output, valueToken, targetToken, dataContext);
            }
        }
    }

    private void ApplyOutput(DecisionOutput output, JToken value, JToken targetToken, JToken dataContext)
    {
        try
        {
            var outputPath = output.Path;
            JToken valueToken = value;

            // Handle relative vs absolute paths for output
            if (outputPath.StartsWith("@."))
            {
                // Relative path - apply to current target token
                var relativePath = outputPath.Substring(2); // Remove "@."
                var pathParts = relativePath.Split('.');

                var currentToken = targetToken;
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    if (currentToken is JObject obj)
                    {
                        if (!obj.ContainsKey(pathParts[i]))
                            obj[pathParts[i]] = new JObject();
                        currentToken = obj[pathParts[i]];
                    }
                }

                if (currentToken is JObject finalObj)
                {
                    finalObj[pathParts.Last()] = valueToken;
                }
            }
            else
            {
                // Absolute path - use framework's path creation
                var targetPath = JsonPathMethods.SplitPath(outputPath);
                JsonMethods.CheckOrCreateParentPath(dataContext, targetPath,
                    executionContext.ItemsFetcher, executionContext.Logger);

                var targets = executionContext.ItemsFetcher.SelectTokens(
                    targetPath.ParentElements.ToPathString(), dataContext);

                foreach (var target in targets)
                {
                    if (target is JObject obj)
                        obj[targetPath.LastName] = valueToken;
                }
            }

            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"Output '{output.Name}' set to: {value} at path: {output.Path}");
        }
        catch (Exception ex)
        {
            executionContext.LogError(CoreConstants.CommandExecution,
                $"Failed to apply output '{output.Name}': {ex.Message}");
        }
    }
}

// Supporting classes for JSON deserialization
public class DecisionTableConfig
{
    [JsonProperty("inputs")]
    public List<DecisionInput> Inputs { get; set; }

    [JsonProperty("outputs")]
    public List<DecisionOutput> Outputs { get; set; }

    [JsonProperty("rules")]
    public List<DecisionRule> Rules { get; set; }

    [JsonProperty("defaultResults")]
    public Dictionary<string, JToken> DefaultResults { get; set; }

    [JsonProperty("executionStrategy")]
    public ExecutionStrategy ExecutionStrategy { get; set; }
}

public class DecisionInput
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("transform")]
    public string Transform { get; set; }
}

public class DecisionOutput
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}

public class DecisionRule
{
    [JsonProperty("conditions")]

    public Dictionary<string, JToken> Conditions { get; set; }

    [JsonProperty("results")]
    public Dictionary<string, JToken> Results { get; set; }

    [JsonProperty("priority")]
    public int Priority { get; set; }
}

public class ExecutionStrategy
{
    [JsonProperty("mode")]
    public string Mode { get; set; } // "firstMatch", "allMatches", "bestMatch"

    [JsonProperty("conflictResolution")]
    public string ConflictResolution { get; set; } // "priority", "lastWins", "merge"

    [JsonProperty("stopOnError")]
    public bool StopOnError { get; set; }
}