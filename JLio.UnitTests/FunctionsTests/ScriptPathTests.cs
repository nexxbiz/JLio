using System.IO;
using System.Linq;
using System.Reflection;
using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using JLio.Extensions.ETL;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests;

public class ScriptPathTests
{
    private IExecutionContext executionContext;
    private IParseOptions parseOptions;
    private string testDataPath;

    [SetUp]
    public void Setup()
    {
        parseOptions = ParseOptions.CreateDefault().RegisterMath().RegisterText().RegisterETL();
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
        var testCase = LoadTestCase("at-symbol-only.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithAbsolutePath_ReturnsAbsolutePath()
    {
        var testCase = LoadTestCase("absolute-path.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_WithEmptyStringArgument_ReturnsCurrentPath()
    {
        var testCase = LoadTestCase("empty-string-argument.json");
        ExecuteTestCase(testCase);
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

    [Test]
    public void PathFunction_BooksAuthorsExample_WithParentNotationIndirectAndPath()
    {
        var testCase = LoadTestCase("books-authors-example.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void PathFunction_ContextualPathBehavior_WithCustomerExample()
    {
        // This test verifies the correct behavior of @.id vs @ when processing arrays
        
        // Arrange - Customer data similar to documentation example
        var customerData = JToken.Parse(@"{
            ""customers"": [
                {""id"": ""C001"", ""name"": ""Alice Johnson"", ""type"": ""premium""},
                {""id"": ""C002"", ""name"": ""Bob Smith"", ""type"": ""standard""}
            ]
        }");

        // Test 1: Using =path(@.id) - should return path to the id property
        var script1 = @"[{
            ""path"": ""$.customers[*].idPath"",
            ""value"": ""=path(@.id)"",
            ""command"": ""add""
        }]";

        var result1 = JLioConvert.Parse(script1, parseOptions).Execute(customerData.DeepClone(), executionContext);
        
        Assert.IsTrue(result1.Success);
        Assert.AreEqual("$.customers[0].id", result1.Data.SelectToken("$.customers[0].idPath")?.Value<string>());
        Assert.AreEqual("$.customers[1].id", result1.Data.SelectToken("$.customers[1].idPath")?.Value<string>());

        // Test 2: Using =path() - should return path to current customer object
        var script2 = @"[{
            ""path"": ""$.customers[*].customerPath"",
            ""value"": ""=path()"",
            ""command"": ""add""
        }]";

        var result2 = JLioConvert.Parse(script2, parseOptions).Execute(customerData.DeepClone(), executionContext);
        
        Assert.IsTrue(result2.Success);
        Assert.AreEqual("$.customers[0]", result2.Data.SelectToken("$.customers[0].customerPath")?.Value<string>());
        Assert.AreEqual("$.customers[1]", result2.Data.SelectToken("$.customers[1].customerPath")?.Value<string>());

        // Test 3: Using =path(@) - should return path to current customer object (same as path())
        var script3 = @"[{
            ""path"": ""$.customers[*].currentPath"",
            ""value"": ""=path(@)"",
            ""command"": ""add""
        }]";

        var result3 = JLioConvert.Parse(script3, parseOptions).Execute(customerData.DeepClone(), executionContext);
        
        Assert.IsTrue(result3.Success);
        Assert.AreEqual("$.customers[0]", result3.Data.SelectToken("$.customers[0].currentPath")?.Value<string>());
        Assert.AreEqual("$.customers[1]", result3.Data.SelectToken("$.customers[1].currentPath")?.Value<string>());
    }

    [Test]
    public void PathFunction_DocumentationExampleCorrection_CustomerIdPath()
    {
        // This test demonstrates the CORRECT way to get customer object paths
        // The documentation example was misleading
        
        var customerData = JToken.Parse(@"{
            ""customers"": [
                {""id"": ""C001"", ""name"": ""Alice Johnson"", ""type"": ""premium""},
                {""id"": ""C002"", ""name"": ""Bob Smith"", ""type"": ""standard""}
            ]
        }");

        // CORRECT: To get the path to each customer object, use =path() or =path(@)
        var correctScript = @"[{
            ""path"": ""$.customers[*].customerIdPath"",
            ""value"": ""=path()"",
            ""command"": ""add""
        }]";

        var result = JLioConvert.Parse(correctScript, parseOptions).Execute(customerData, executionContext);
        
        Assert.IsTrue(result.Success);
        
        // This should give us the path to each customer object, not the id property
        Assert.AreEqual("$.customers[0]", result.Data.SelectToken("$.customers[0].customerIdPath")?.Value<string>());
        Assert.AreEqual("$.customers[1]", result.Data.SelectToken("$.customers[1].customerIdPath")?.Value<string>());
        
        // Verify the objects still have their original data
        Assert.AreEqual("C001", result.Data.SelectToken("$.customers[0].id")?.Value<string>());
        Assert.AreEqual("Alice Johnson", result.Data.SelectToken("$.customers[0].name")?.Value<string>());
        Assert.AreEqual("C002", result.Data.SelectToken("$.customers[1].id")?.Value<string>());
        Assert.AreEqual("Bob Smith", result.Data.SelectToken("$.customers[1].name")?.Value<string>());
    }

    [Test]
    public void PathFunction_PropertyPath_VsObjectPath()
    {
        // Demonstrate the difference between getting property path vs object path
        
        var testData = JToken.Parse(@"{
            ""orders"": [
                {""orderId"": ""ORD-001"", ""customerId"": ""C001"", ""total"": 150.00}
            ]
        }");

        // Test getting property paths
        var propertyScript = @"[
            {
                ""path"": ""$.orders[*].orderIdPath"",
                ""value"": ""=path(@.orderId)"",
                ""command"": ""add""
            },
            {
                ""path"": ""$.orders[*].customerIdPath"",
                ""value"": ""=path(@.customerId)"",
                ""command"": ""add""
            }
        ]";

        var propertyResult = JLioConvert.Parse(propertyScript, parseOptions).Execute(testData, executionContext);
        
        Assert.IsTrue(propertyResult.Success);
        Assert.AreEqual("$.orders[0].orderId", propertyResult.Data.SelectToken("$.orders[0].orderIdPath")?.Value<string>());
        Assert.AreEqual("$.orders[0].customerId", propertyResult.Data.SelectToken("$.orders[0].customerIdPath")?.Value<string>());

        // Test getting object path
        var objectScript = @"[{
            ""path"": ""$.orders[*].orderPath"",
            ""value"": ""=path()"",
            ""command"": ""add""
        }]";

        var objectResult = JLioConvert.Parse(objectScript, parseOptions).Execute(testData.DeepClone(), executionContext);
        
        Assert.IsTrue(objectResult.Success);
        Assert.AreEqual("$.orders[0]", objectResult.Data.SelectToken("$.orders[0].orderPath")?.Value<string>());
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