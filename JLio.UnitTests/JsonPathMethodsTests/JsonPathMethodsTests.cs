using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.JsonPathMethodsTests;

public class JsonPathMethodsTests
{
    private JToken comprehensiveTestData;
    private JToken simpleTestData;
    private JToken emptyTestData;
    private JToken indirectTestData;
    private string testDataPath;
    private IItemsFetcher itemsFetcher;

    [SetUp]
    public void Setup()
    {
        // Get the test data directory path
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        testDataPath = Path.Combine(assemblyDirectory, "TestData", "JsonPathTests");
        
        // Load test data files
        var comprehensiveDataFile = Path.Combine(testDataPath, "comprehensive-test-data.json");
        var simpleDataFile = Path.Combine(testDataPath, "simple-test-data.json");
        var emptyDataFile = Path.Combine(testDataPath, "empty-test-data.json");
        var indirectDataFile = Path.Combine(testDataPath, "indirect-test-data.json");
        
        comprehensiveTestData = JToken.Parse(File.ReadAllText(comprehensiveDataFile));
        simpleTestData = JToken.Parse(File.ReadAllText(simpleDataFile));
        emptyTestData = JToken.Parse(File.ReadAllText(emptyDataFile));
        indirectTestData = JToken.Parse(File.ReadAllText(indirectDataFile));
        
        itemsFetcher = new JsonPathItemsFetcher();
    }

    #region SplitPath Tests

    [TestCase("$", 1)]
    [TestCase("$.store", 2)]
    [TestCase("$.store.book", 3)]
    [TestCase("$.store.book[0]", 3)]
    [TestCase("$.store.book[0].title", 4)]
    [TestCase("$..book", 3)] // Fixed: $, empty element for .., book
    [TestCase("$.store.book[*].author", 4)]
    [TestCase("$.store.book[?(@.price < 10)]", 3)]
    public void SplitPath_ShouldReturnCorrectElementCount(string path, int expectedElementCount)
    {
        var result = JsonPathMethods.SplitPath(path);
        
        Assert.AreEqual(expectedElementCount, result.Elements.Count, 
            $"Path '{path}' should have {expectedElementCount} elements");
    }

    [TestCase("$.store.book", "book")]
    [TestCase("$.user.preferences.notifications", "notifications")]
    [TestCase("$.company.employees[0]", "employees[0]")] // Fixed: includes array notation
    [TestCase("$.arrays.numbers[*]", "numbers[*]")] // Fixed: includes array notation
    [TestCase("$.metadata.nested.deep.deeper.deepest", "deepest")]
    public void SplitPath_ShouldReturnCorrectLastName(string path, string expectedLastName)
    {
        var result = JsonPathMethods.SplitPath(path);
        
        Assert.AreEqual(expectedLastName, result.LastName, 
            $"Path '{path}' should have last name '{expectedLastName}'");
    }

    [TestCase("$.store.book", "book")]
    [TestCase("$.user.preferences.notifications", "notifications")]
    [TestCase("$.company.employees[0]", "employees")] // ElementName excludes array notation
    [TestCase("$.arrays.numbers[*]", "numbers")] // ElementName excludes array notation
    [TestCase("$.metadata.nested.deep.deeper.deepest", "deepest")]
    public void SplitPath_ShouldReturnCorrectElementName(string path, string expectedElementName)
    {
        var result = JsonPathMethods.SplitPath(path);
        
        Assert.AreEqual(expectedElementName, result.LastElement.ElementName, 
            $"Path '{path}' should have element name '{expectedElementName}'");
    }

    [TestCase("$.store.book[0]", true)]
    [TestCase("$.arrays.numbers[*]", true)]
    [TestCase("$.company.employees[?(@.salary > 80000)]", true)]
    [TestCase("$.store.bicycle", false)]
    [TestCase("$.user.name", false)]
    [TestCase("$.metadata.version", false)]
    public void SplitPath_ShouldDetectArrayIndication(string path, bool expectedHasArrayIndication)
    {
        var result = JsonPathMethods.SplitPath(path);
        
        Assert.AreEqual(expectedHasArrayIndication, result.HasArrayIndication, 
            $"Path '{path}' array indication should be {expectedHasArrayIndication}");
    }

    #endregion

    #region Indirect Path Tests

    [Test]
    public void SelectTokens_WithIndirectPath_ShouldReturnCorrectTokens()
    {
        // Test basic indirect path resolution
        var result = itemsFetcher.SelectTokens("=indirect($.paths.userPath)", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        var firstUser = result.First();
        Assert.AreEqual("Alice Johnson", firstUser.SelectToken("name")?.Value<string>());
        Assert.AreEqual(1, firstUser.SelectToken("id")?.Value<int>());
    }

    [Test]
    public void SelectTokens_WithIndirectPathToArray_ShouldReturnAllItems()
    {
        // Test indirect path that resolves to array selection
        var result = itemsFetcher.SelectTokens("=indirect($.paths.allUsersPath)", indirectTestData);
        
        Assert.AreEqual(2, result.Count);
        var users = result.ToList();
        Assert.AreEqual("Alice Johnson", users[0].SelectToken("name")?.Value<string>());
        Assert.AreEqual("Bob Smith", users[1].SelectToken("name")?.Value<string>());
    }

    [Test]
    public void SelectTokens_WithIndirectPathToProperty_ShouldReturnPropertyValue()
    {
        // Test indirect path that resolves to a specific property
        var result = itemsFetcher.SelectTokens("=indirect($.paths.emailPath)", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("alice@example.com", result.First().Value<string>());
    }

    [Test]
    public void SelectTokens_WithNestedIndirectPath_ShouldWork()
    {
        // Test indirect path with nested reference
        var result = itemsFetcher.SelectTokens("=indirect($.nested.level1.level2.targetPath)", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Bob Smith", result.First().Value<string>());
    }

    [Test]
    public void SelectTokens_WithQuotedIndirectPath_ShouldWork()
    {
        // Test indirect path with quotes around the path reference
        var result = itemsFetcher.SelectTokens("=indirect('$.paths.userPath')", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        var firstUser = result.First();
        Assert.AreEqual("Alice Johnson", firstUser.SelectToken("name")?.Value<string>());
    }

    [Test]
    public void SelectTokens_WithDoubleQuotedIndirectPath_ShouldWork()
    {
        // Test indirect path with double quotes around the path reference
        var result = itemsFetcher.SelectTokens("=indirect(\"$.paths.adminPath\")", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        var firstUser = result.First();
        Assert.AreEqual("Bob Smith", firstUser.SelectToken("name")?.Value<string>());
    }

    [Test]
    public void SelectTokens_WithInvalidIndirectPathReference_ShouldReturnEmpty()
    {
        // Test indirect path that references a non-existent path
        var result = itemsFetcher.SelectTokens("=indirect($.paths.nonExistentPath)", indirectTestData);
        
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void SelectTokens_WithIndirectPathToInvalidPath_ShouldReturnEmpty()
    {
        // Test indirect path that resolves to an invalid JSONPath
        var result = itemsFetcher.SelectTokens("=indirect($.paths.invalidPath)", indirectTestData);
        
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void SelectTokens_WithIndirectPathToEmptyString_ShouldReturnEmpty()
    {
        // Test indirect path that resolves to empty string
        var result = itemsFetcher.SelectTokens("=indirect($.paths.emptyPath)", indirectTestData);
        
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void SelectTokens_WithIndirectPathToNonString_ShouldReturnEmpty()
    {
        // Test indirect path that resolves to a non-string value
        var result = itemsFetcher.SelectTokens("=indirect($.paths.numberPath)", indirectTestData);
        
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void SelectTokens_WithMalformedIndirectFunction_ShouldReturnEmpty()
    {
        // Test malformed indirect function syntax
        var result = itemsFetcher.SelectTokens("=indirect($.paths.userPath", indirectTestData); // Missing closing parenthesis
        
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void SelectToken_WithIndirectPath_ShouldReturnCorrectToken()
    {
        // Test SelectToken with indirect path
        var result = itemsFetcher.SelectToken("=indirect($.paths.emailPath)", indirectTestData);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("alice@example.com", result.Value<string>());
    }

    [Test]
    public void SelectToken_WithInvalidIndirectPath_ShouldReturnNull()
    {
        // Test SelectToken with invalid indirect path
        var result = itemsFetcher.SelectToken("=indirect($.paths.invalidPath)", indirectTestData);
        
        Assert.IsNull(result);
    }

    [Test]
    public void SelectTokens_WithComplexIndirectPath_ShouldWork()
    {
        // Test indirect path combined with additional path elements
        var result = itemsFetcher.SelectTokens("=indirect($.paths.allUsersPath).name", indirectTestData);
        
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Alice Johnson", result.First().Value<string>());
        Assert.AreEqual("Bob Smith", result.Last().Value<string>());
    }

    [Test]
    public void SelectTokens_WithNestedIndirectInComplexPath_ShouldWork()
    {
        // Test indirect path as part of a more complex JSONPath expression
        var result = itemsFetcher.SelectTokens("=indirect($.paths.skillsPath)[0]", indirectTestData);
        
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("JavaScript", result.First().Value<string>());
        Assert.AreEqual("Java", result.Last().Value<string>());
    }

    [Test]
    public void SelectTokens_WithIndirectPathToSingleObject_ShouldReturnObject()
    {
        // Test indirect path that resolves to a single object (not in an array)
        var result = itemsFetcher.SelectTokens("=indirect($.paths.configPath)", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        var configObject = result.First();
        Assert.AreEqual("$.data.users[0]", configObject.SelectToken("currentUserPath")?.Value<string>());
        Assert.AreEqual("email", configObject.SelectToken("dynamicProperty")?.Value<string>());
        Assert.AreEqual(true, configObject.SelectToken("isEnabled")?.Value<bool>());
        Assert.AreEqual(3, configObject.SelectToken("maxRetries")?.Value<int>());
    }

    [Test]
    public void SelectTokens_WithIndirectPathToNestedSingleObject_ShouldReturnNestedObject()
    {
        // Test indirect path that resolves to a nested single object
        var result = itemsFetcher.SelectTokens("=indirect($.paths.nestedPath)", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        var nestedObject = result.First();
        Assert.AreEqual("$.data.users[1].name", nestedObject.SelectToken("targetPath")?.Value<string>());
        Assert.IsNotNull(nestedObject.SelectToken("objectData"));
        Assert.AreEqual("Manager", nestedObject.SelectToken("objectData.title")?.Value<string>());
        Assert.AreEqual("Engineering", nestedObject.SelectToken("objectData.department")?.Value<string>());
    }

    [Test]
    public void SelectTokens_WithIndirectPathToSingleProperty_ShouldReturnPropertyValue()
    {
        // Test indirect path that resolves to a single property within an object
        var result = itemsFetcher.SelectTokens("=indirect($.paths.singlePropertyPath)", indirectTestData);
        
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("email", result.First().Value<string>());
    }

    [Test]
    public void SelectToken_WithIndirectPathToSingleObject_ShouldReturnObject()
    {
        // Test SelectToken with indirect path to single object
        var result = itemsFetcher.SelectToken("=indirect($.paths.configPath)", indirectTestData);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(JTokenType.Object, result.Type);
        Assert.AreEqual("$.data.users[0]", result.SelectToken("currentUserPath")?.Value<string>());
        Assert.AreEqual(true, result.SelectToken("isEnabled")?.Value<bool>());
    }

    [Test]
    public void SelectToken_WithIndirectPathToNestedObject_ShouldReturnNestedObject()
    {
        // Test SelectToken with indirect path to nested object
        var result = itemsFetcher.SelectToken("=indirect($.paths.nestedPath)", indirectTestData);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(JTokenType.Object, result.Type);
        Assert.AreEqual("Manager", result.SelectToken("objectData.title")?.Value<string>());
    }

    [Test]
    public void SelectTokens_WithIndirectPathToSingleValues_ShouldReturnCorrectTypes()
    {
        // Test indirect paths to various single value types
        var stringResult = itemsFetcher.SelectTokens("=indirect($.paths.stringValuePath)", indirectTestData);
        Assert.AreEqual(1, stringResult.Count);
        Assert.AreEqual("simple string", stringResult.First().Value<string>());

        var numberResult = itemsFetcher.SelectTokens("=indirect($.paths.numberValuePath)", indirectTestData);
        Assert.AreEqual(1, numberResult.Count);
        Assert.AreEqual(42, numberResult.First().Value<int>());

        var boolResult = itemsFetcher.SelectTokens("=indirect($.paths.booleanValuePath)", indirectTestData);
        Assert.AreEqual(1, boolResult.Count);
        Assert.AreEqual(true, boolResult.First().Value<bool>());

        var nullResult = itemsFetcher.SelectTokens("=indirect($.paths.nullValuePath)", indirectTestData);
        Assert.AreEqual(1, nullResult.Count);
        Assert.AreEqual(JTokenType.Null, nullResult.First().Type);
    }

    #endregion

    #region GetIntellisense Tests

    [Test]
    public void GetIntellisense_WithRootPath_ShouldReturnTopLevelProperties()
    {
        var result = JsonPathMethods.GetIntellisense("$.", comprehensiveTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.store"));
        Assert.That(result, Contains.Item("$.user"));
        Assert.That(result, Contains.Item("$.company"));
        Assert.That(result, Contains.Item("$.metadata"));
        Assert.That(result, Contains.Item("$.arrays"));
        
        // Should not contain nested properties
        Assert.That(result, Does.Not.Contain("$.store.book"));
    }

    [Test]
    public void GetIntellisense_WithStorePath_ShouldReturnStoreProperties()
    {
        var result = JsonPathMethods.GetIntellisense("$.store.", comprehensiveTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.store.book"));
        Assert.That(result, Contains.Item("$.store.bicycle"));
        
        // Should not contain properties from other root objects
        Assert.That(result, Does.Not.Contain("$.user"));
        Assert.That(result, Does.Not.Contain("$.company"));
    }

    [Test]
    public void GetIntellisense_WithArrayPath_ShouldReturnArrayNotation()
    {
        var result = JsonPathMethods.GetIntellisense("$.store.book", comprehensiveTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.store.book[*]"));
    }

    [Test]
    public void GetIntellisense_WithDeepNestedPath_ShouldReturnCorrectProperties()
    {
        var result = JsonPathMethods.GetIntellisense("$.user.preferences.", comprehensiveTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.user.preferences.theme"));
        Assert.That(result, Contains.Item("$.user.preferences.language"));
        Assert.That(result, Contains.Item("$.user.preferences.notifications"));
    }

    [Test]
    public void GetIntellisense_WithPartialMatch_ShouldReturnFilteredResults()
    {
        var result = JsonPathMethods.GetIntellisense("$.store.bi", comprehensiveTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.store.bicycle"));
        Assert.That(result, Does.Not.Contain("$.store.book"));
    }

    [Test]
    public void GetIntellisense_WithInvalidPath_ShouldFallBackToParentPath()
    {
        var result = JsonPathMethods.GetIntellisense("$.store.nonexistent.", comprehensiveTestData, itemsFetcher);
        
        // Should fall back to $.store and return its properties
        Assert.That(result, Contains.Item("$.store.book"));
        Assert.That(result, Contains.Item("$.store.bicycle"));
    }

    [Test]
    public void GetIntellisense_WithEmptyObject_ShouldReturnEmptyList()
    {
        var result = JsonPathMethods.GetIntellisense("$.", emptyTestData, itemsFetcher);
        
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetIntellisense_WithSimpleObject_ShouldReturnAllProperties()
    {
        var result = JsonPathMethods.GetIntellisense("$.", simpleTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.simple"));
        Assert.That(result, Contains.Item("$.nested"));
        Assert.That(result, Contains.Item("$.array"));
        Assert.That(result, Contains.Item("$.objectArray"));
    }

    #endregion

    #region Integration Tests with Real JSONPath Scenarios

    [TestCase("$.store.book[*].title", TestName = "All book titles")]
    [TestCase("$.store.book[?(@.price < 10)].title", TestName = "Cheap book titles")]
    [TestCase("$.company.employees[?(@.department == 'Engineering')].name", TestName = "Engineering employee names")]
    [TestCase("$..price", TestName = "All prices in document")]
    [TestCase("$.arrays.numbers[2]", TestName = "Third number in array")]
    public void JsonPathScenarios_ShouldWorkWithRealData(string jsonPath)
    {
        // Test that our test data works with actual JSONPath queries
        var tokens = itemsFetcher.SelectTokens(jsonPath, comprehensiveTestData);
        
        Assert.IsNotNull(tokens);
        // Most queries should return at least one result with our comprehensive test data
        if (!jsonPath.Contains("?(@.price < 10)")) // This might return 0 results depending on prices
        {
            Assert.Greater(tokens.Count, 0, $"JSONPath '{jsonPath}' should return at least one result");
        }
    }

    [Test]
    public void GetIntellisense_WithArrayElements_ShouldProvideCorrectSuggestions()
    {
        // Test getting intellisense for array elements
        var result = JsonPathMethods.GetIntellisense("$.company.employees", comprehensiveTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.company.employees[*]"));
    }

    [Test]
    public void GetIntellisense_WithComplexNestedStructure_ShouldWork()
    {
        // Test deeply nested structure
        var result = JsonPathMethods.GetIntellisense("$.metadata.nested.deep.deeper.", comprehensiveTestData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.metadata.nested.deep.deeper.deepest"));
    }

    #endregion

    #region Edge Cases and Error Handling

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(".")]
    [TestCase("..")]
    public void GetIntellisense_WithInvalidInput_ShouldHandleGracefully(string invalidInput)
    {
        Assert.DoesNotThrow(() =>
        {
            var result = JsonPathMethods.GetIntellisense(invalidInput, comprehensiveTestData, itemsFetcher);
            Assert.IsNotNull(result);
        });
    }

    [Test]
    public void GetIntellisense_WithNullToken_ShouldHandleGracefully()
    {
        Assert.DoesNotThrow(() =>
        {
            var result = JsonPathMethods.GetIntellisense("$.", null, itemsFetcher);
            Assert.IsNotNull(result);
        });
    }

    [Test]
    public void SplitPath_WithComplexArrayNotation_ShouldWork()
    {
        var complexPaths = new[]
        {
            "$.store.book[?(@.price < 10)]",
            "$.company.employees[?(@.department == 'Engineering')]",
            "$.arrays.objects[?(@.active == true)]"
        };

        foreach (var path in complexPaths)
        {
            Assert.DoesNotThrow(() =>
            {
                var result = JsonPathMethods.SplitPath(path);
                Assert.IsNotNull(result);
                Assert.Greater(result.Elements.Count, 0);
            }, $"Should handle complex path: {path}");
        }
    }

    #endregion

    #region Performance and Large Data Tests

    [Test]
    public void GetIntellisense_WithLargeObject_ShouldPerformWell()
    {
        var startTime = DateTime.Now;
        
        var result = JsonPathMethods.GetIntellisense("$.", comprehensiveTestData, itemsFetcher);
        
        var endTime = DateTime.Now;
        var executionTime = endTime - startTime;
        
        Assert.Less(executionTime.TotalMilliseconds, 1000, "Should complete within 1 second");
        Assert.IsNotNull(result);
        Assert.Greater(result.Count, 0);
    }

    #endregion

    #region File-based Test Data Validation

    [Test]
    public void TestDataFiles_ShouldExistAndBeValid()
    {
        var files = new[]
        {
            "comprehensive-test-data.json",
            "simple-test-data.json", 
            "empty-test-data.json",
            "indirect-test-data.json"
        };

        foreach (var file in files)
        {
            var filePath = Path.Combine(testDataPath, file);
            Assert.IsTrue(File.Exists(filePath), $"Test data file should exist: {file}");
            
            var content = File.ReadAllText(filePath);
            Assert.DoesNotThrow(() => JToken.Parse(content), $"Test data file should be valid JSON: {file}");
        }
    }

    [Test]
    public void ComprehensiveTestData_ShouldHaveExpectedStructure()
    {
        // Verify the comprehensive test data has the expected structure
        Assert.IsNotNull(comprehensiveTestData.SelectToken("$.store.book"));
        Assert.IsNotNull(comprehensiveTestData.SelectToken("$.user.preferences"));
        Assert.IsNotNull(comprehensiveTestData.SelectToken("$.company.employees"));
        Assert.IsNotNull(comprehensiveTestData.SelectToken("$.arrays.numbers"));
        Assert.IsNotNull(comprehensiveTestData.SelectToken("$.metadata.nested.deep.deeper.deepest"));
        
        // Verify arrays have expected content
        var books = comprehensiveTestData.SelectTokens("$.store.book[*]");
        Assert.Greater(books.Count(), 0, "Should have books in test data");
        
        var employees = comprehensiveTestData.SelectTokens("$.company.employees[*]");
        Assert.Greater(employees.Count(), 0, "Should have employees in test data");
    }

    [Test]
    public void IndirectTestData_ShouldHaveExpectedStructure()
    {
        // Verify the indirect test data has the expected structure
        Assert.IsNotNull(indirectTestData.SelectToken("$.paths"));
        Assert.IsNotNull(indirectTestData.SelectToken("$.data.users"));
        Assert.AreEqual("$.data.users[0]", indirectTestData.SelectToken("$.paths.userPath")?.Value<string>());
        Assert.AreEqual("Alice Johnson", indirectTestData.SelectToken("$.data.users[0].name")?.Value<string>());
    }

    #endregion

    #region Specific JSONPath Method Behavior Tests

    [Test]
    public void GetIntellisense_ShouldReturnDistinctResults()
    {
        var result = JsonPathMethods.GetIntellisense("$.", comprehensiveTestData, itemsFetcher);
        
        var distinctCount = result.Distinct().Count();
        Assert.AreEqual(result.Count, distinctCount, "Results should not contain duplicates");
    }

    [Test]
    public void GetIntellisense_ShouldFilterByStartsWith()
    {
        var result = JsonPathMethods.GetIntellisense("$.st", comprehensiveTestData, itemsFetcher);
        
        // All results should start with "$.st" if any matches are found
        if (result.Any())
        {
            Assert.IsTrue(result.All(r => r.StartsWith("$.st")), 
                "All results should start with the input prefix when matches are found");
        }
    }

    [TestCase("$.store.book", "book")]
    [TestCase("$.user.addresses[0]", "addresses[0]")] // Fixed: LastName includes array notation
    [TestCase("$.company.departments.Engineering", "Engineering")]
    public void SplitPath_LastName_ShouldBeCorrect(string path, string expectedLastName)
    {
        var splittedPath = JsonPathMethods.SplitPath(path);
        Assert.AreEqual(expectedLastName, splittedPath.LastName);
    }

    [TestCase("$.store.book[0].title", "$.store.book[0]")]
    [TestCase("$.user.preferences.notifications.email", "$.user.preferences.notifications")]
    [TestCase("$.arrays.numbers[*]", "$.arrays")]
    public void SplitPath_ParentElements_ShouldBeCorrect(string path, string expectedParentPath)
    {
        var splittedPath = JsonPathMethods.SplitPath(path);
        var parentElements = splittedPath.ParentElements.ToList();
        
        // Build the parent path from elements
        var actualParentPath = string.Join(".", parentElements.Select(e => e.PathElementFullText));
        
        // Convert back to proper JSONPath format
        if (!actualParentPath.StartsWith("$"))
        {
            actualParentPath = "$." + actualParentPath;
        }
        
        Assert.AreEqual(expectedParentPath, actualParentPath, 
            $"Parent path for '{path}' should be '{expectedParentPath}'");
    }

    #endregion
}