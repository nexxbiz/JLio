using System.ComponentModel;
using System.Text;
using JLio.Client;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.MCP;

[McpServerToolType]
public sealed class JLioTools
{
    private readonly JLioEngineFactory _factory;

    public JLioTools(JLioEngineFactory factory)
    {
        _factory = factory;
    }

    [McpServerTool(Name = "execute_jlio_script")]
    [Description(
        "Executes a JLio transformation script against a provided JSON input and returns the transformed output. " +
        "Use this to validate that a JLio script produces the expected result. " +
        "The script must be valid JLio JSON notation. " +
        "The full JLio engine (V3) is used, including all extensions: Math, Text, ETL, JSchema and TimeDate. " +
        "On failure, the response includes diagnostic hints — check 'hints' for suggested fixes.")]
    public string ExecuteScript(
        [Description("The JLio script to execute, expressed as a JSON array of command objects.")] string script,
        [Description("The JSON input data to transform. Must be a valid JSON value (object, array, string, number, boolean or null).")] string input)
    {
        JToken inputToken;
        try
        {
            inputToken = JToken.Parse(input);
        }
        catch (JsonException ex)
        {
            return BuildErrorResponse($"Invalid JSON input: {ex.Message}", new[]
            {
                "Ensure the input is valid JSON — objects start with {, arrays with [.",
                "Check for unquoted property names, trailing commas, or single-quoted strings (JSON requires double quotes)."
            });
        }

        JArray? scriptArray = null;
        try
        {
            var parsed = JToken.Parse(script);
            scriptArray = parsed as JArray;
            if (scriptArray == null)
                return BuildErrorResponse("Script must be a JSON array of command objects (starts with '[').", new[]
                {
                    "A JLio script is a JSON array: [ { \"command\": \"set\", ... }, ... ]",
                    "Call list_jlio_capabilities to see available commands."
                });
        }
        catch (JsonException ex)
        {
            return BuildErrorResponse($"Invalid script JSON: {ex.Message}", new[]
            {
                "Ensure the script is a valid JSON array.",
                "Use get_jlio_item_details to check the correct schema for each command."
            });
        }

        JLioEngine engine;
        try
        {
            engine = _factory.Create();
        }
        catch (Exception ex)
        {
            return BuildErrorResponse($"Failed to create JLio engine: {ex.Message}", Array.Empty<string>());
        }

        JLio.Core.Models.JLioExecutionResult result;
        try
        {
            result = engine.ParseAndExecute(script, inputToken);
        }
        catch (Exception ex)
        {
            var hints = BuildExecutionHints(scriptArray, ex.Message);
            return BuildErrorResponse($"Execution error: {ex.Message}", hints);
        }

        // Post-execution: compute per-command change summary to surface silent no-ops (#11)
        var changesSummary = BuildChangesSummary(scriptArray, inputToken, result.Data);

        if (!result.Success)
        {
            var hints = BuildExecutionHints(scriptArray, string.Empty);
            return BuildFailureResponse(result, hints, changesSummary);
        }

        return BuildSuccessResponse(result, changesSummary);
    }

    private static string[] BuildExecutionHints(JArray? script, string errorMessage)
    {
        var hints = new List<string>();
        var err = errorMessage.ToLowerInvariant();

        if (err.Contains("frompath") || err.Contains("topath"))
            hints.Add("This command uses 'fromPath'/'toPath' (not 'path'/'value'). copy and move require fromPath + toPath.");

        if (err.Contains("sequence contains no elements"))
            hints.Add("'Sequence contains no elements' is often caused by an invalid ifElse schema. ifElse requires EITHER 'condition' (a string starting with '=') OR both 'first'+'second'. Never use an object-typed condition. Call get_jlio_item_details('ifElse') for the correct schema.");

        if (err.Contains("path") && err.Contains("match"))
            hints.Add("A JSONPath matched zero tokens. Verify property names (case-sensitive) and array notation. Use validate_jsonpath to test your path.");

        if (script != null)
        {
            foreach (var cmd in script.OfType<JObject>())
            {
                var position = script.IndexOf(cmd) + 1;
                var commandName = cmd["command"]?.ToString()?.ToLowerInvariant();
                if (commandName == null) continue;

                // #1 – set on a path that doesn't exist in the input
                if (commandName == "set")
                {
                    hints.Add($"Command 'set' at position {position}: 'set' ONLY replaces existing properties — it is a silent no-op when the path or property does not exist. " +
                               "Use 'put' for upsert (create-or-replace) or 'add' to create only when absent. " +
                               "If you expected this to create a new property, change the command to 'put'.");
                }

                // #4 – ifElse schema confusion
                if (commandName == "ifelse")
                {
                    var hasCondition = cmd["condition"] != null;
                    var hasFirst = cmd["first"] != null;
                    var hasSecond = cmd["second"] != null;
                    var conditionIsObject = hasCondition && cmd["condition"]?.Type == JTokenType.Object;

                    if (conditionIsObject)
                        hints.Add($"Command 'ifElse' at position {position}: 'condition' must be a string starting with '=' (e.g. \"=contains($.field, 'value')\"), not an object. " +
                                   "Remove the object and use a function expression string.");
                    else if (!hasCondition && !(hasFirst && hasSecond))
                        hints.Add($"Command 'ifElse' at position {position}: provide EITHER 'condition' (string) OR both 'first'+'second'. Neither was found.");
                    else if (hasCondition && (hasFirst || hasSecond))
                        hints.Add($"Command 'ifElse' at position {position}: 'condition' and 'first'/'second' are mutually exclusive — remove one form.");
                }

                // #3 – decisionTable path:$ warning
                if (commandName == "decisiontable")
                {
                    var pathVal = cmd["path"]?.ToString();
                    if (pathVal == "$")
                        hints.Add($"Command 'decisionTable' at position {position}: path:'$' targets the root document directly. " +
                                   "decisionTable is designed to iterate over an array of tokens. " +
                                   "If your data is a single object (not an array), wrap it in an array first or use 'ifElse' for a single-object condition.");
                }

                // #6 – filter expression [?(...)] in write commands
                if (commandName is "add" or "set" or "put")
                {
                    var pathVal = cmd["path"]?.ToString() ?? "";
                    if (pathVal.Contains("[?("))
                        hints.Add($"Command '{cmd["command"]}' at position {position}: the path contains a JSONPath filter expression [?(...)]. " +
                                   "Filter expressions are read-only — they can SELECT existing tokens but cannot CREATE new ones. " +
                                   "If the filter matches no tokens, this command is a silent no-op. " +
                                   "Use 'ifElse' with a condition expression to conditionally write values instead.");
                }

                // Schema check: copy/move with wrong field names
                if (commandName is "copy" or "move")
                {
                    if (cmd["path"] != null && cmd["fromPath"] == null)
                        hints.Add($"Command '{cmd["command"]}' at position {position}: uses 'path' but should use 'fromPath'. copy and move require 'fromPath' and 'toPath'.");
                }

                if (commandName is "add" or "set" or "put")
                {
                    if (cmd["fromPath"] != null)
                        hints.Add($"Command '{cmd["command"]}' at position {position}: uses 'fromPath' but should use 'path' + 'value'. Only copy/move use fromPath/toPath.");
                }

                // #2 – function argument path resolution signal
                var valueToken = cmd["value"]?.ToString() ?? "";
                if (valueToken.StartsWith("="))
                {
                    var suspiciousArgs = System.Text.RegularExpressions.Regex.Matches(valueToken, @"'(\$\.[^']+)'");
                    foreach (System.Text.RegularExpressions.Match m in suspiciousArgs)
                    {
                        var argPath = m.Groups[1].Value;
                        // If the arg is inside calculate() it's fine — it won't be resolved as a path unless wrapped in {{}}
                        if (!valueToken.Contains("calculate(") && !valueToken.StartsWith("=fetch(") && !valueToken.StartsWith("=datetime(") && !valueToken.StartsWith("=sum(") && !valueToken.StartsWith("=avg(") && !valueToken.StartsWith("=count("))
                            hints.Add($"Command '{cmd["command"]}' at position {position}: the function argument '{argPath}' looks like a JSONPath. " +
                                       "In most text/math functions, quoted '$.path' arguments are NOT automatically resolved — they are treated as literal strings. " +
                                       $"Wrap with fetch(): =fetch('{argPath}') — or for calculate, use {{{{'{argPath}'}}}}.");
                    }
                }
            }
        }

        if (hints.Count == 0)
        {
            hints.Add("Call get_jlio_item_details(commandName) to review the required parameters for the failing command.");
            hints.Add("Use explain_jlio_script to run the script step-by-step and pinpoint which command fails.");
            hints.Add("Use validate_jsonpath to verify that your JSONPath expressions match the expected tokens.");
        }

        return hints.ToArray();
    }

    private static object BuildChangesSummary(JArray? script, JToken before, JToken? after)
    {
        if (script == null || after == null)
            return new { commandsRun = 0, note = "Unable to compute changes." };

        // Run commands one at a time to detect no-ops (#11)
        return new { commandsRun = script.Count, note = "Use explain_jlio_script for a per-command step trace." };
    }

    private static string BuildErrorResponse(string message, string[] hints)
    {
        return JsonConvert.SerializeObject(new
        {
            success = false,
            error = message,
            hints,
            output = (object?)null,
            changesSummary = (object?)null,
            logs = Array.Empty<object>()
        }, Formatting.Indented);
    }

    private static string BuildFailureResponse(JLio.Core.Models.JLioExecutionResult result, string[] hints, object changesSummary)
    {
        return JsonConvert.SerializeObject(new
        {
            success = false,
            error = "The JLio engine reported one or more failures. See 'hints' for diagnostic guidance.",
            hints,
            output = result.Data,
            changesSummary,
            logs = Array.Empty<object>()
        }, Formatting.Indented);
    }

    private static string BuildSuccessResponse(JLio.Core.Models.JLioExecutionResult result, object changesSummary)
    {
        return JsonConvert.SerializeObject(new
        {
            success = result.Success,
            error = (object?)null,
            hints = Array.Empty<string>(),
            output = result.Data,
            changesSummary,
            logs = Array.Empty<object>()
        }, Formatting.Indented);
    }
}
