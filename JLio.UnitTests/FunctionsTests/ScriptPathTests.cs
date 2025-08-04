using System.IO;
using System.Linq;
using System.Reflection;
using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class ScriptPathTests
{
    private IExecutionContext executionContext;
    private ParseOptions parseOptions;
    private string testDataPath;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault();
        executionContext = ExecutionContext.CreateDefault();
        
        // Get the test data directory path
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        testDataPath = Path.Combine(assemblyDirectory, "TestData", "ScriptPathTests");
    }

    [Test]
    public void PathFunction_WithNoArguments_ReturnsCurrentTokenPath()
    {
        // Load test files
        var inputFile = Path.Combine(testDataPath, "basic-path-data.json");
        var expectedFile = Path.Combine(testDataPath, "basic-path-expected.json");
        var scriptFile = Path.Combine(testDataPath, "basic-path-script.json");

        var data = JToken.Parse(File.ReadAllText(inputFile));
        var expected = JToken.Parse(File.ReadAllText(expectedFile));
        var script = File.ReadAllText(scriptFile);

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(expected, result.Data), 
            $"Expected: {expected}\nActual: {result.Data}");
    }

    [Test]
    public void PathFunction_WithRelativePathArgument_ReturnsAbsolutePath()
    {
        // Load test files
        var inputFile = Path.Combine(testDataPath, "relative-path-data.json");
        var expectedFile = Path.Combine(testDataPath, "relative-path-expected.json");
        var scriptFile = Path.Combine(testDataPath, "relative-path-script.json");

        var data = JToken.Parse(File.ReadAllText(inputFile));
        var expected = JToken.Parse(File.ReadAllText(expectedFile));
        var script = File.ReadAllText(scriptFile);

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(expected, result.Data), 
            $"Expected: {expected}\nActual: {result.Data}");
    }

    [Test]
    public void PathFunction_ComplexExample_WithSecondPath()
    {
        // Load test files - this matches the user's example
        var inputFile = Path.Combine(testDataPath, "complex-example-data.json");
        var expectedFile = Path.Combine(testDataPath, "complex-example-expected.json");
        var scriptFile = Path.Combine(testDataPath, "complex-example-script.json");

        var data = JToken.Parse(File.ReadAllText(inputFile));
        var expected = JToken.Parse(File.ReadAllText(expectedFile));
        var script = File.ReadAllText(scriptFile);

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(expected, result.Data), 
            $"Expected: {expected}\nActual: {result.Data}");
    }

    [Test]
    public void PathFunction_AtRootLevel_ReturnsRootPath()
    {
        // Load test files
        var inputFile = Path.Combine(testDataPath, "root-level-data.json");
        var expectedFile = Path.Combine(testDataPath, "root-level-expected.json");
        var scriptFile = Path.Combine(testDataPath, "root-level-script.json");

        var data = JToken.Parse(File.ReadAllText(inputFile));
        var expected = JToken.Parse(File.ReadAllText(expectedFile));
        var script = File.ReadAllText(scriptFile);

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(expected, result.Data), 
            $"Expected: {expected}\nActual: {result.Data}");
    }

    [Test]
    public void PathFunction_WithAtSymbolOnly_ReturnsCurrentPath()
    {
        // Arrange - using inline data for simple @ test
        var data = JObject.Parse(@"{
            ""sample"": {
                ""myArray"": [
                    { ""myItem"": ""value1"" }
                ]
            }
        }");

        var script = @"[
            {
                ""path"": ""$.sample.myArray[*].selfPath"",
                ""value"": ""=path(@)"",
                ""command"": ""add""
            }
        ]";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.sample.myArray[0]", result.Data.SelectToken("$.sample.myArray[0].selfPath")?.Value<string>());
    }

    [Test]
    public void PathFunction_WithAbsolutePath_ReturnsAbsolutePath()
    {
        // Arrange - using inline data for absolute path test
        var data = JObject.Parse(@"{
            ""sample"": {
                ""myArray"": [
                    { ""myItem"": ""value1"" }
                ]
            }
        }");

        var script = @"[
            {
                ""path"": ""$.sample.myArray[*].absolutePath"",
                ""value"": ""=path($.some.absolute.path)"",
                ""command"": ""add""
            }
        ]";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.some.absolute.path", result.Data.SelectToken("$.sample.myArray[0].absolutePath")?.Value<string>());
    }

    [Test]
    public void PathFunction_WithEmptyStringArgument_ReturnsCurrentPath()
    {
        // Arrange - using inline data for empty string test
        var data = JObject.Parse(@"{
            ""sample"": {
                ""myArray"": [
                    { ""myItem"": ""value1"" }
                ]
            }
        }");

        var script = @"[
            {
                ""path"": ""$.sample.myArray[*].currentPath"",
                ""value"": ""=path('')"",
                ""command"": ""add""
            }
        ]";

        // Act
        var result = JLioConvert.Parse(script, parseOptions).Execute(data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.sample.myArray[0]", result.Data.SelectToken("$.sample.myArray[0].currentPath")?.Value<string>());
    }

    [Test]
    public void PathFunction_DirectlyInCode_WithNoArguments_ReturnsCurrentTokenPath()
    {
        // Arrange
        var pathFunction = new ScriptPath();
        var currentToken = JToken.Parse(@"{ ""test"": ""value"" }");
        var currentTokenAtPath = currentToken.SelectToken("$.test");

        // Act
        var result = pathFunction.Execute(currentTokenAtPath, currentToken, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.test", result.Data.First().Value<string>());
    }

    [Test]
    public void PathFunction_DirectlyInCode_WithRelativePath_ReturnsAbsolutePath()
    {
        // Arrange
        var pathFunction = new ScriptPath("@.childProperty");
        var currentToken = JToken.Parse(@"{ ""parent"": { ""childProperty"": ""value"" } }");
        var currentTokenAtPath = currentToken.SelectToken("$.parent");

        // Act
        var result = pathFunction.Execute(currentTokenAtPath, currentToken, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.parent.childProperty", result.Data.First().Value<string>());
    }

    [Test]
    public void PathFunction_DirectlyInCode_WithAbsolutePath_ReturnsAbsolutePath()
    {
        // Arrange
        var pathFunction = new ScriptPath("$.some.absolute.path");
        var currentToken = JToken.Parse(@"{ ""test"": ""value"" }");
        var currentTokenAtPath = currentToken.SelectToken("$.test");

        // Act
        var result = pathFunction.Execute(currentTokenAtPath, currentToken, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.some.absolute.path", result.Data.First().Value<string>());
    }

    [Test]
    public void PathFunction_FunctionName_IsPath()
    {
        // Arrange
        var pathFunction = new ScriptPath();

        // Act & Assert
        Assert.AreEqual("path", pathFunction.FunctionName);
    }
}