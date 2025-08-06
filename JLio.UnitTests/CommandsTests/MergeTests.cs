using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JLio.Commands.Advanced;
using JLio.Commands.Advanced.Builders;
using JLio.Commands.Advanced.Settings;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;


namespace JLio.UnitTests.CommandsTests;

public class MergeTests
{
    private IExecutionContext executeOptions;
    private string testDataPath;

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        
        // Get the test data directory path
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        testDataPath = Path.Combine(assemblyDirectory, "TestData", "MergeTests");
    }

    [Test]
    public void MergeTwoComplexArraysUnique()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexArraysUniqueWithComplexKeys()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique-with-complex-keys.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexArraysUniqueWithoutComplexKeys()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique-without-complex-keys.json");
        ExecuteTestCase(testCase, expectFailure: true);
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeys()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique-with-keys.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexArraysUniqueWithoutKeys()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique-without-keys.json");
        ExecuteTestCase(testCase, expectFailure: true);
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeysAndOnlyStructure()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique-with-keys-and-only-structure.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeysAndOnlyValues()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique-with-keys-and-only-values.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeysWithDoubleDotNotations()
    {
        var testCase = LoadTestCase("merge-two-complex-arrays-unique-with-keys-with-double-dot-notations.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexPropertiesInObject()
    {
        var testCase = LoadTestCase("merge-two-complex-properties-in-object.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexPropertiesInObjectOnlyStructure()
    {
        var testCase = LoadTestCase("merge-two-complex-properties-in-object-only-structure.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void MergeTwoComplexPropertiesInObjectOnlyValues()
    {
        var testCase = LoadTestCase("merge-two-complex-properties-in-object-only-values.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void CanUseFluentApi()
    {
        var script = new JLioScript()
                .Merge("$.first").With("$.result").UsingDefaultSettings()
                .Merge("$.first").With("$.result").Using(new MergeSettings())
            ;
        var result = script.Execute(new JObject());

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }

    private TestCaseData LoadTestCase(string fileName)
    {
        var filePath = Path.Combine(testDataPath, fileName);
        var jsonContent = File.ReadAllText(filePath);
        var testCaseJson = JObject.Parse(jsonContent);

        return new TestCaseData
        {
            Data = testCaseJson["data"]?.DeepClone(),
            Expected = testCaseJson["expected"]?.DeepClone(),
            Path = testCaseJson["path"]?.Value<string>(),
            TargetPath = testCaseJson["targetPath"]?.Value<string>(),
            MergeSettings = ParseMergeSettings(testCaseJson["mergeSettings"])
        };
    }

    private MergeSettings ParseMergeSettings(JToken mergeSettingsToken)
    {
        if (mergeSettingsToken == null || mergeSettingsToken.Type == JTokenType.Null) return null;

        var settings = new MergeSettings();
        
        if (mergeSettingsToken["strategy"] != null)
        {
            settings.Strategy = mergeSettingsToken["strategy"].Value<string>();
        }

        if (mergeSettingsToken["arraySettings"] != null)
        {
            settings.ArraySettings = new List<MergeArraySettings>();
            foreach (var arraySettingToken in mergeSettingsToken["arraySettings"])
            {
                var arraySettings = new MergeArraySettings
                {
                    ArrayPath = arraySettingToken["arrayPath"]?.Value<string>(),
                    UniqueItemsWithoutKeys = arraySettingToken["uniqueItemsWithoutKeys"]?.Value<bool>() ?? false
                };

                if (arraySettingToken["keyPaths"] != null)
                {
                    arraySettings.KeyPaths = arraySettingToken["keyPaths"].ToObject<List<string>>();
                }

                settings.ArraySettings.Add(arraySettings);
            }
        }

        if (mergeSettingsToken["matchSettings"] != null)
        {
            settings.MatchSettings = new MatchSettings();
            if (mergeSettingsToken["matchSettings"]["keyPaths"] != null)
            {
                settings.MatchSettings.KeyPaths = mergeSettingsToken["matchSettings"]["keyPaths"].ToObject<List<string>>();
            }
        }

        return settings;
    }

    private void ExecuteTestCase(TestCaseData testCase, bool expectFailure = false)
    {
        // Act
        Merge mergeCommand;
        if (testCase.MergeSettings == null)
        {
            mergeCommand = new Merge(testCase.Path, testCase.TargetPath);
        }
        else
        {
            mergeCommand = new Merge(testCase.Path, testCase.TargetPath, testCase.MergeSettings);
        }
        
        var result = mergeCommand.Execute(testCase.Data, executeOptions);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void CanMergeComplexArrayIntoEmptyArray()
    {
        var testData = JToken.Parse(@"{
        ""sourceArray"": [
            {""id"": 1, ""name"": ""Item1"", ""data"": {""value"": ""complex1""}},
            {""id"": 2, ""name"": ""Item2"", ""data"": {""value"": ""complex2""}}
        ],
        ""targetArray"": []
    }");

        var mergeCommand = new Merge("$.sourceArray", "$.targetArray");
        var result = mergeCommand.Execute(testData, executeOptions);

        // Verify the empty array now contains all complex items
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, testData.SelectTokens("$.targetArray[*]").Count());
        // Add more specific validations...
    }

    [Test]
    public void CanMergeComplexArrayIntofullArrayMakingItDistinct()
    {
        var testData = JToken.Parse(@"{
        ""sourceArray"": [
            {""id"": 1, ""name"": ""Item1"", ""data"": {""value"": ""complex1""}},
            {""id"": 2, ""name"": ""Item2"", ""data"": {""value"": ""complex2""}}
        ],
        ""targetArray"": []
    }");

        var mergeCommand = new Merge("$.sourceArray", "$.targetArray");
        var result = mergeCommand.Execute(testData, executeOptions);

        // Verify the empty array now contains all complex items
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, testData.SelectTokens("$.targetArray[*]").Count());
        // Add more specific validations...
    }

    [Test]
    public void CanMergeComplexArrayWithDuplicatesIntoEmptyArrayMakingDistinct()
    {
        var testData = JToken.Parse(@"{
        ""sourceArray"": [
            {""id"": 1, ""name"": ""Item1"", ""data"": {""value"": ""complex1""}, ""sourceProperty"": ""first occurrence""},
            {""id"": 2, ""name"": ""Item2"", ""data"": {""value"": ""complex2""}, ""sourceProperty"": ""unique item""},
            {""id"": 1, ""name"": ""Item1Updated"", ""data"": {""value"": ""complex1Updated""}, ""anotherProperty"": ""second occurrence""},
            {""id"": 3, ""name"": ""Item3"", ""data"": {""value"": ""complex3""}, ""sourceProperty"": ""another unique""},
            {""id"": 2, ""name"": ""Item2Modified"", ""data"": {""value"": ""complex2Modified""}, ""extraProperty"": ""duplicate of id 2""}
        ],
        ""targetArray"": []
    }");

        var mergeSettings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
        {
            new MergeArraySettings
            {
                ArrayPath = "$.targetArray",
                KeyPaths = new List<string> {"id"}
            }
        }
        };

        var mergeCommand = new Merge("$.sourceArray", "$.targetArray", mergeSettings);
        var result = mergeCommand.Execute(testData, executeOptions);

        // Verify the merge was successful
        Assert.IsTrue(result.Success);

        // Verify we have only 3 distinct items (ids 1, 2, 3) instead of 5
        Assert.AreEqual(3, testData.SelectTokens("$.targetArray[*]").Count());

        // Verify each unique ID is present
        Assert.AreEqual(1, testData.SelectTokens("$.targetArray[?(@.id == 1)]").Count());
        Assert.AreEqual(1, testData.SelectTokens("$.targetArray[?(@.id == 2)]").Count());
        Assert.AreEqual(1, testData.SelectTokens("$.targetArray[?(@.id == 3)]").Count());

        // Verify merged properties for ID 1 (should have properties from both occurrences)
        var item1 = testData.SelectToken("$.targetArray[?(@.id == 1)]");
        Assert.IsNotNull(item1);
        Assert.AreEqual("Item1Updated", item1.SelectToken("name")?.Value<string>()); // Last occurrence wins for conflicting properties
        Assert.AreEqual("complex1Updated", item1.SelectToken("data.value")?.Value<string>());
        Assert.AreEqual("first occurrence", item1.SelectToken("sourceProperty")?.Value<string>()); // Property from first occurrence preserved
        Assert.AreEqual("second occurrence", item1.SelectToken("anotherProperty")?.Value<string>()); // Property from second occurrence added

        // Verify merged properties for ID 2
        var item2 = testData.SelectToken("$.targetArray[?(@.id == 2)]");
        Assert.IsNotNull(item2);
        Assert.AreEqual("Item2Modified", item2.SelectToken("name")?.Value<string>());
        Assert.AreEqual("complex2Modified", item2.SelectToken("data.value")?.Value<string>());
        Assert.AreEqual("unique item", item2.SelectToken("sourceProperty")?.Value<string>()); // From first occurrence
        Assert.AreEqual("duplicate of id 2", item2.SelectToken("extraProperty")?.Value<string>()); // From second occurrence

        // Verify ID 3 (unique item)
        var item3 = testData.SelectToken("$.targetArray[?(@.id == 3)]");
        Assert.IsNotNull(item3);
        Assert.AreEqual("Item3", item3.SelectToken("name")?.Value<string>());
        Assert.AreEqual("complex3", item3.SelectToken("data.value")?.Value<string>());
        Assert.AreEqual("another unique", item3.SelectToken("sourceProperty")?.Value<string>());
    }

    [Test]
    public void CanMergeComplexArrayWithDuplicatesOnSameArrayMakingDistinct()
    {
        var testData = JToken.Parse(@"{
        ""sourceArray"": [
            {""id"": 1, ""name"": ""Item1"", ""data"": {""value"": ""complex1""}, ""sourceProperty"": ""first occurrence""},
            {""id"": 2, ""name"": ""Item2"", ""data"": {""value"": ""complex2""}, ""sourceProperty"": ""unique item""},
            {""id"": 1, ""name"": ""Item1Updated"", ""data"": {""value"": ""complex1Updated""}, ""anotherProperty"": ""second occurrence""},
            {""id"": 3, ""name"": ""Item3"", ""data"": {""value"": ""complex3""}, ""sourceProperty"": ""another unique""},
            {""id"": 2, ""name"": ""Item2Modified"", ""data"": {""value"": ""complex2Modified""}, ""extraProperty"": ""duplicate of id 2""}
        ]
    }");

        var mergeSettings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
        {
            new MergeArraySettings
            {
                ArrayPath = "$.sourceArray",
                KeyPaths = new List<string> {"id"}
            }
        }
        };

        var mergeCommand = new Merge("$.sourceArray", "$.sourceArray", mergeSettings);
        var result = mergeCommand.Execute(testData, executeOptions);

        // Verify the merge failed because source and target paths are the same
        Assert.IsFalse(result.Success, "Merge should fail when source and target paths are identical");

        // Verify the original data remains unchanged (5 items still present)
        Assert.AreEqual(5, testData.SelectTokens("$.sourceArray[*]").Count(),
            "Original array should remain unchanged when merge fails");

        // Verify validation warning was logged
        var warnings = executeOptions.GetLogEntries()
            .Where(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Warning)
            .ToList();

        Assert.IsTrue(warnings.Any(w => w.Message.Contains("Path and TargetPath for merge command cannot be the same")),
            "Should log validation warning about identical paths");
    }
}