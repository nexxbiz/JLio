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
        var testCase = LoadTestCase("basic-path.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithRelativePathArgument_ReturnsAbsolutePath()
    {
        var testCase = LoadTestCase("relative-path.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_ComplexExample_WithSecondPath()
    {
        var testCase = LoadTestCase("complex-example.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_AtRootLevel_ReturnsRootPath()
    {
        var testCase = LoadTestCase("root-level.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithParentPathIndicator_ReturnsCurrentItemPath()
    {
        var testCase = LoadTestCase("parent-path.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithAtSymbolOnly_ReturnsCurrentPath()
    {
        // Arrange - using inline test case structure
        var testCase = new TestCaseData
        {
            Data = JObject.Parse(@"{
                ""sample"": {
                    ""myArray"": [
                        { ""myItem"": ""value1"" }
                    ]
                }
            }"),
            Script = @"[
                {
                    ""path"": ""$.sample.myArray[*].selfPath"",
                    ""value"": ""=path(@)"",
                    ""command"": ""add""
                }
            ]"
        };

        // Act
        var result = JLioConvert.Parse(testCase.Script, parseOptions).Execute(testCase.Data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.sample.myArray[0]", result.Data.SelectToken("$.sample.myArray[0].selfPath")?.Value<string>());
    }

    [Test]
    public void PathFunction_WithAbsolutePath_ReturnsAbsolutePath()
    {
        // Arrange - using inline test case structure
        var testCase = new TestCaseData
        {
            Data = JObject.Parse(@"{
                ""sample"": {
                    ""myArray"": [
                        { ""myItem"": ""value1"" }
                    ]
                }
            }"),
            Script = @"[
                {
                    ""path"": ""$.sample.myArray[*].absolutePath"",
                    ""value"": ""=path($.some.absolute.path)"",
                    ""command"": ""add""
                }
            ]"
        };

        // Act
        var result = JLioConvert.Parse(testCase.Script, parseOptions).Execute(testCase.Data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("$.some.absolute.path", result.Data.SelectToken("$.sample.myArray[0].absolutePath")?.Value<string>());
    }

    [Test]
    public void PathFunction_WithEmptyStringArgument_ReturnsCurrentPath()
    {
        // Arrange - using proper JSON escaping like other working tests
        var script = @"[{""path"":""$.sample.myArray[*].currentPath"",""value"":""=path('')"",""command"":""add""}]";
        
        var data = JObject.Parse(@"{
            ""sample"": {
                ""myArray"": [
                    { ""myItem"": ""value1"" }
                ]
            }
        }");

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

    private TestCaseData LoadTestCase(string fileName)
    {
        var filePath = Path.Combine(testDataPath, fileName);
        var jsonContent = File.ReadAllText(filePath);
        var testCaseJson = JObject.Parse(jsonContent);

        return new TestCaseData
        {
            Data = testCaseJson["data"]?.DeepClone(),
            Script = testCaseJson["script"]?.ToString(),
            Expected = testCaseJson["expected"]?.DeepClone()
        };
    }

    private void ExecuteTestCase(TestCaseData testCase)
    {
        // Act
        var result = JLioConvert.Parse(testCase.Script, parseOptions).Execute(testCase.Data, executionContext);

        // Assert
        Assert.IsTrue(result.Success);
        if (testCase.Expected != null)
        {
            Assert.IsTrue(JToken.DeepEquals(testCase.Expected, result.Data), 
                $"Expected: {testCase.Expected}\nActual: {result.Data}");
        }
    }

    private class TestCaseData
    {
        public JToken Data { get; set; }
        public string Script { get; set; }
        public JToken Expected { get; set; }
    }
}