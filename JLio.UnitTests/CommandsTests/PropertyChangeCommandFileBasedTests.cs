using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.Math;
using JLio.Extensions.Text;
using JLio.Extensions.ETL;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class PropertyChangeCommandFileBasedTests
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
        testDataPath = Path.Combine(assemblyDirectory, "TestData", "PropertyChangeTests");
    }

    #region Add Command Tests

    [TestCase("add-legacy-simple", TestName = "Add Legacy Simple")]
    [TestCase("add-new-simple", TestName = "Add New Simple")]
    [TestCase("add-legacy-path-function", TestName = "Add Legacy Path Function")]
    [TestCase("add-new-path-function", TestName = "Add New Path Function")]
    [TestCase("add-legacy-array", TestName = "Add Legacy Array")]
    [TestCase("add-new-array", TestName = "Add New Array")]
    [TestCase("add-legacy-parent-navigation", TestName = "Add Legacy Parent Navigation")]
    [TestCase("add-new-parent-navigation", TestName = "Add New Parent Navigation")]
    public void AddCommandFileBasedTests(string testCaseName)
    {
        ExecuteFileBasedTest(testCaseName);
    }

    #endregion

    #region Set Command Tests

    [TestCase("set-legacy-simple", TestName = "Set Legacy Simple")]
    [TestCase("set-new-simple", TestName = "Set New Simple")]
    [TestCase("set-legacy-function", TestName = "Set Legacy Function")]
    [TestCase("set-new-function", TestName = "Set New Function")]
    public void SetCommandFileBasedTests(string testCaseName)
    {
        ExecuteFileBasedTest(testCaseName);
    }

    #endregion

    #region Put Command Tests

    [TestCase("put-legacy-create", TestName = "Put Legacy Create")]
    [TestCase("put-new-create", TestName = "Put New Create")]
    [TestCase("put-legacy-update", TestName = "Put Legacy Update")]
    [TestCase("put-new-update", TestName = "Put New Update")]
    public void PutCommandFileBasedTests(string testCaseName)
    {
        ExecuteFileBasedTest(testCaseName);
    }

    #endregion

    #region Syntax Comparison Tests

    [TestCase("add-legacy-simple", "add-new-simple", TestName = "Add Syntax Comparison Simple")]
    [TestCase("add-legacy-path-function", "add-new-path-function", TestName = "Add Syntax Comparison Path Function")]
    [TestCase("add-legacy-parent-navigation", "add-new-parent-navigation", TestName = "Add Syntax Comparison Parent Navigation")]
    [TestCase("set-legacy-simple", "set-new-simple", TestName = "Set Syntax Comparison Simple")]
    [TestCase("set-legacy-function", "set-new-function", TestName = "Set Syntax Comparison Function")]
    [TestCase("put-legacy-create", "put-new-create", TestName = "Put Syntax Comparison Create")]
    [TestCase("put-legacy-update", "put-new-update", TestName = "Put Syntax Comparison Update")]
    public void SyntaxComparisonTests(string legacyTestCase, string newTestCase)
    {
        // Load and execute legacy syntax test
        var legacyResult = LoadAndExecuteTestCase(legacyTestCase);
        
        // Load and execute new syntax test
        var newResult = LoadAndExecuteTestCase(newTestCase);
        
        // Both should succeed
        Assert.IsTrue(legacyResult.Success, $"Legacy syntax test failed: {legacyTestCase}");
        Assert.IsTrue(newResult.Success, $"New syntax test failed: {newTestCase}");
        
        // Results should be identical
        var areEqual = JToken.DeepEquals(legacyResult.Data, newResult.Data);
        
        if (!areEqual)
        {
            Console.WriteLine($"Legacy result for {legacyTestCase}:");
            Console.WriteLine(legacyResult.Data.ToString());
            Console.WriteLine($"New result for {newTestCase}:");
            Console.WriteLine(newResult.Data.ToString());
        }
        
        Assert.IsTrue(areEqual, $"Results should be identical between legacy ({legacyTestCase}) and new ({newTestCase}) syntax");
    }

    #endregion

    #region Test Execution Helper Methods

    private void ExecuteFileBasedTest(string testCaseName)
    {
        // Load the test file
        var testFile = Path.Combine(testDataPath, $"{testCaseName}.json");
        Assert.IsTrue(File.Exists(testFile), $"Test file not found: {testFile}");

        var testCaseJson = JObject.Parse(File.ReadAllText(testFile));
        var inputData = testCaseJson["data"]?.DeepClone();
        var script = testCaseJson["script"]?.ToString();
        var expectedData = testCaseJson["expected"]?.DeepClone();

        Assert.IsNotNull(inputData, $"Test case {testCaseName} missing 'data' section");
        Assert.IsNotNull(script, $"Test case {testCaseName} missing 'script' section");
        Assert.IsNotNull(expectedData, $"Test case {testCaseName} missing 'expected' section");

        // Execute the script
        var parsedScript = JLioConvert.Parse(script, parseOptions);
        var result = parsedScript.Execute(inputData, executionContext);

        // Assert execution was successful
        Assert.IsTrue(result.Success, $"Script execution failed for test case: {testCaseName}");

        // Verify no errors in execution context
        var errors = executionContext.Logger.LogEntries
            .Where(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error).ToList();
        Assert.IsEmpty(errors, 
            $"Execution errors found in {testCaseName}: {string.Join("; ", errors.Select(e => e.Message))}");

        // Compare results using deep equality
        var actualResult = result.Data;
        var areEqual = JToken.DeepEquals(expectedData, actualResult);

        if (!areEqual)
        {
            Console.WriteLine($"Expected for {testCaseName}:");
            Console.WriteLine(expectedData.ToString());
            Console.WriteLine($"Actual for {testCaseName}:");
            Console.WriteLine(actualResult.ToString());
        }

        Assert.IsTrue(areEqual, $"Results do not match expected output for test case: {testCaseName}");
    }

    private JLioExecutionResult LoadAndExecuteTestCase(string testCaseName)
    {
        // Load the test file
        var testFile = Path.Combine(testDataPath, $"{testCaseName}.json");
        var testCaseJson = JObject.Parse(File.ReadAllText(testFile));
        var inputData = testCaseJson["data"]?.DeepClone();
        var script = testCaseJson["script"]?.ToString();

        // Execute the script
        var parsedScript = JLioConvert.Parse(script, parseOptions);
        return parsedScript.Execute(inputData, ExecutionContext.CreateDefault());
    }

    #endregion

    #region Test Data Validation Tests

    [Test]
    public void AllTestDataFilesExistAndAreValid()
    {
        var expectedTestFiles = new[]
        {
            // Add command tests
            "add-legacy-simple.json",
            "add-new-simple.json",
            "add-legacy-path-function.json",
            "add-new-path-function.json",
            "add-legacy-array.json",
            "add-new-array.json",
            "add-legacy-parent-navigation.json",
            "add-new-parent-navigation.json",
            
            // Set command tests
            "set-legacy-simple.json",
            "set-new-simple.json",
            "set-legacy-function.json",
            "set-new-function.json",
            
            // Put command tests
            "put-legacy-create.json",
            "put-new-create.json",
            "put-legacy-update.json",
            "put-new-update.json"
        };

        foreach (var fileName in expectedTestFiles)
        {
            var filePath = Path.Combine(testDataPath, fileName);
            Assert.IsTrue(File.Exists(filePath), $"Test data file should exist: {fileName}");

            var content = File.ReadAllText(filePath);
            Assert.DoesNotThrow(() => JObject.Parse(content), 
                $"Test data file should be valid JSON: {fileName}");

            var testCase = JObject.Parse(content);
            Assert.IsNotNull(testCase["data"], $"Test file {fileName} should have 'data' section");
            Assert.IsNotNull(testCase["script"], $"Test file {fileName} should have 'script' section");
            Assert.IsNotNull(testCase["expected"], $"Test file {fileName} should have 'expected' section");
        }
    }

    [Test]
    public void TestDataDirectoryExists()
    {
        Assert.IsTrue(Directory.Exists(testDataPath), 
            $"Test data directory should exist: {testDataPath}");
    }

    #endregion
}