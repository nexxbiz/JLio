using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JLio.Client;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.ETL;
using JLio.Extensions.ETL.Commands;
using JLio.Extensions.ETL.Commands.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests.ETLTests
{
    [TestFixture]
    public class FlattenRestoreTests
    {
        private IExecutionContext executionContext;
        private IParseOptions parseOptions;
        private string testDataPath;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault().RegisterETL();
            executionContext = ExecutionContext.CreateDefault();
            
            // Get the test data directory path
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            testDataPath = Path.Combine(assemblyDirectory, "TestData", "ETLTests");
        }

        [TestCase("simple-roundtrip", TestName = "Simple Flatten and Restore Roundtrip")]
        [TestCase("complex-nested-arrays-flatten", TestName = "Complex Nested Arrays Flattening")]
        [TestCase("complex-nested-arrays-restore", TestName = "Complex Nested Arrays Restore")]
        [TestCase("csv-like-structure", TestName = "CSV-like Structure with JSONPath")]
        [TestCase("empty-arrays", TestName = "Empty Arrays Handling")]
        [TestCase("custom-delimiters", TestName = "Custom Delimiters Test")]
        public void FlattenRestoreFileBasedTests(string testCaseName)
        {
            // Load test files
            var inputFile = Path.Combine(testDataPath, $"{testCaseName}-input.json");
            var expectedFile = Path.Combine(testDataPath, $"{testCaseName}-expected.json");
            var scriptFile = Path.Combine(testDataPath, $"{testCaseName}-script.json");

            Assert.IsTrue(File.Exists(inputFile), $"Input file not found: {inputFile}");
            Assert.IsTrue(File.Exists(expectedFile), $"Expected file not found: {expectedFile}");
            Assert.IsTrue(File.Exists(scriptFile), $"Script file not found: {scriptFile}");

            // Load data
            var inputData = JToken.Parse(File.ReadAllText(inputFile));
            var expectedData = JToken.Parse(File.ReadAllText(expectedFile));
            var script = File.ReadAllText(scriptFile);

            // Execute the script
            var parsedScript = JLioConvert.Parse(script, parseOptions);
            var result = parsedScript.Execute(inputData, executionContext);

            // Assert execution was successful
            Assert.IsTrue(result.Success, $"Script execution failed for test case: {testCaseName}");
            
            // Verify no errors in execution context
            var errors = executionContext.Logger.LogEntries.Where(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error).ToList();
            Assert.IsEmpty(errors, $"Execution errors found: {string.Join("; ", errors.Select(e => e.Message))}");

            // Compare results using deep equality (excluding dynamic fields like timestamp)
            var actualResult = result.Data;
            var areEqual = CompareResultsIgnoringDynamicFields(expectedData, actualResult);
            
            if (!areEqual)
            {
                Console.WriteLine($"Expected for {testCaseName}:");
                Console.WriteLine(expectedData.ToString());
                Console.WriteLine($"Actual for {testCaseName}:");
                Console.WriteLine(actualResult.ToString());
            }
            
            Assert.IsTrue(areEqual, $"Results do not match expected output for test case: {testCaseName}");
        }

        private bool CompareResultsIgnoringDynamicFields(JToken expected, JToken actual)
        {
            // Create deep clones to avoid modifying original data
            var expectedClone = expected.DeepClone();
            var actualClone = actual.DeepClone();
            
            // Remove dynamic fields that change between test runs
            RemoveDynamicFields(expectedClone);
            RemoveDynamicFields(actualClone);
            
            return JToken.DeepEquals(expectedClone, actualClone);
        }

        private void RemoveDynamicFields(JToken token)
        {
            if (token is JObject obj)
            {
                // Remove timestamp fields that will vary between runs
                var metadataTokens = obj.SelectTokens("$.._flattenMetadata").Cast<JObject>()
                    .Concat(obj.SelectTokens("$.._metadata").Cast<JObject>())
                    .Concat(obj.SelectTokens("$.._structure").Cast<JObject>())
                    .Concat(obj.SelectTokens("$.._meta").Cast<JObject>());
                
                foreach (var metadata in metadataTokens)
                {
                    metadata.Remove("timestamp");
                    metadata.Remove("rootPath"); // This might vary based on execution context
                }
                
                // Recursively process child objects
                foreach (var property in obj.Properties().ToList())
                {
                    RemoveDynamicFields(property.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array)
                {
                    RemoveDynamicFields(item);
                }
            }
        }

        [Test]
        public void CanValidateFlattenCommand()
        {
            var flattenCommand = new Flatten
            {
                Path = "", // Empty path to trigger validation error
                FlattenSettings = null // Null settings to trigger validation error
            };

            var validationResult = flattenCommand.ValidateCommandInstance();

            Assert.IsFalse(validationResult.IsValid);
            Assert.Contains("Path property is required for flatten command", validationResult.ValidationMessages);
            Assert.Contains("FlattenSettings property is required for flatten command", validationResult.ValidationMessages);
        }

        [Test]
        public void CanValidateRestoreCommand()
        {
            var restoreCommand = new Restore
            {
                Path = "", // Empty path to trigger validation error
                RestoreSettings = new RestoreSettings
                {
                    Delimiter = "", // Empty delimiter to trigger validation error
                    UseJsonPathColumn = true,
                    JsonPathColumn = "" // Empty JSONPath column when required
                }
            };

            var validationResult = restoreCommand.ValidateCommandInstance();

            Assert.IsFalse(validationResult.IsValid);
            Assert.Contains("Path property is required for restore command", validationResult.ValidationMessages);
            Assert.Contains("Delimiter cannot be empty in RestoreSettings", validationResult.ValidationMessages);
            Assert.Contains("JsonPathColumn cannot be empty when UseJsonPathColumn is true", validationResult.ValidationMessages);
        }
    }
}