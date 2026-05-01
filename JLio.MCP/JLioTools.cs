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
        "The full JLio engine (V3) is used, including all extensions: Math, Text, ETL, JSchema and TimeDate.")]
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
            return BuildErrorResponse($"Invalid JSON input: {ex.Message}");
        }

        JLioEngine engine;
        try
        {
            engine = _factory.Create();
        }
        catch (Exception ex)
        {
            return BuildErrorResponse($"Failed to create JLio engine: {ex.Message}");
        }

        JLio.Core.Models.JLioExecutionResult result;
        try
        {
            result = engine.ParseAndExecute(script, inputToken);
        }
        catch (Exception ex)
        {
            return BuildErrorResponse($"Execution error: {ex.Message}");
        }

        return BuildSuccessResponse(result);
    }

    private static string BuildErrorResponse(string message)
    {
        return JsonConvert.SerializeObject(new
        {
            success = false,
            error = message,
            output = (object?)null,
            logs = Array.Empty<object>()
        }, Formatting.Indented);
    }

    private static string BuildSuccessResponse(JLio.Core.Models.JLioExecutionResult result)
    {
        return JsonConvert.SerializeObject(new
        {
            success = result.Success,
            error = (object?)null,
            output = result.Data,
            logs = Array.Empty<object>()
        }, Formatting.Indented);
    }
}
