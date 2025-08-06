using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JLio.Commands.Advanced;
using JLio.Commands.Advanced.Builders;
using JLio.Commands.Advanced.Settings;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

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
    public void CanMergeComplexArrayIntoEmptyArray()
    {
        var testCase = LoadTestCase("merge-complex-array-into-empty-array.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void CanMergeComplexArrayIntofullArrayMakingItDistinct()
    {
        var testCase = LoadTestCase("merge-complex-array-into-full-array-making-distinct.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void CanMergeComplexArrayWithDuplicatesIntoEmptyArrayMakingDistinct()
    {
        var testCase = LoadTestCase("merge-complex-array-with-duplicates-into-empty-array-making-distinct.json");
        ExecuteTestCase(testCase);
    }

    [Test]
    public void CanMergeComplexArrayWithDuplicatesOnSameArrayMakingDistinct()
    {
        var testCase = LoadTestCase("merge-complex-array-with-duplicates-on-same-array-making-distinct.json");
        ExecuteTestCase(testCase, true);
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
        var resultText = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data, Formatting.Indented);
        // Assert
        Assert.IsNotNull(result);
        
        if (expectFailure)
        {
            // For tests that expect failure, we might not expect success
            // and we expect the result to not match the expected data
            if (testCase.Expected != null)
            {
                Assert.IsFalse(JToken.DeepEquals(testCase.Expected, result.Data));
            }
        }
        else
        {
            Assert.IsTrue(result.Success);
            if (testCase.Expected != null)
            {
                Assert.IsTrue(JToken.DeepEquals(testCase.Expected, result.Data), 
                    $"Expected: {testCase.Expected}\nActual: {result.Data}");
            }
        }
    }

    private class TestCaseData
    {
        public JToken Data { get; set; }
        public JToken Expected { get; set; }
        public string Path { get; set; }
        public string TargetPath { get; set; }
        public MergeSettings MergeSettings { get; set; }
    }
}