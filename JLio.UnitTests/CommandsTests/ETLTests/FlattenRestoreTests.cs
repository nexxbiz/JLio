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

        [Test]
        public void SimpleRoundtrip_FlattenAndRestore_ShouldReturnOriginalStructure()
        {
            var testCase = LoadTestCase("simple-roundtrip.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void ComplexNestedArraysFlatten_ShouldFlattenCorrectly()
        {
            var testCase = LoadTestCase("complex-nested-arrays-flatten.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void ComplexNestedArraysRestore_ShouldRestoreCorrectly()
        {
            var testCase = LoadTestCase("complex-nested-arrays-restore.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void CsvLikeStructure_ShouldFlattenToCSVFormat()
        {
            var testCase = LoadTestCase("csv-like-structure.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void EmptyArrays_ShouldHandleCorrectly()
        {
            var testCase = LoadTestCase("empty-arrays.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void CustomDelimiters_ShouldUseCorrectDelimiters()
        {
            var testCase = LoadTestCase("custom-delimiters.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void ToCsv_BasicConversion_ShouldGenerateCorrectCsv()
        {
            var testCase = LoadTestCase("to-csv-basic.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void ToCsv_EscapingAndSpecialCharacters_ShouldHandleCorrectly()
        {
            var testCase = LoadTestCase("to-csv-escaping.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void ToCsv_SingleObject_ShouldGenerateOneRowCsv()
        {
            var testCase = LoadTestCase("to-csv-single-object.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void ToCsv_WithTypesAndMetadata_ShouldIncludeAllColumns()
        {
            var testCase = LoadTestCase("to-csv-with-types-and-metadata.json");
            ExecuteTestCase(testCase);
        }

        [Test]
        public void ToCsv_FlattenAndConvert_ShouldWorkTogether()
        {
            // Test the integration of flatten followed by toCsv
            var inputData = JToken.Parse(@"{
                ""products"": [
                    {
                        ""id"": 1,
                        ""info"": {
                            ""name"": ""Laptop"",
                            ""specs"": {
                                ""cpu"": ""Intel i7"",
                                ""ram"": ""16GB""
                            }
                        },
                        ""tags"": [""electronics"", ""computers""]
                    }
                ]
            }");

            var script = @"[
                {
                    ""path"": ""$.products[*]"",
                    ""command"": ""flatten"",
                    ""flattenSettings"": {
                        ""delimiter"": ""."",
                        ""includeArrayIndices"": true,
                        ""metadataPath"": ""$"",
                        ""metadataKey"": ""_metadata""
                    }
                },
                {
                    ""path"": ""$.products[*]"",
                    ""command"": ""toCsv"",
                    ""csvSettings"": {
                        ""delimiter"": "","",
                        ""includeHeaders"": true,
                        ""includeTypeColumns"": false,
                        ""includeMetadata"": false
                    }
                }
            ]";

            var parsedScript = JLioConvert.Parse(script, parseOptions);
            var result = parsedScript.Execute(inputData, executionContext);

            Assert.IsTrue(result.Success, "Script execution failed");
            
            var csvOutput = result.Data.SelectToken("$.products[0]")?.Value<string>();
            Assert.IsNotNull(csvOutput, "CSV output should not be null");
            Assert.IsTrue(csvOutput.Contains("id,info.name,info.specs.cpu,info.specs.ram,tags.0,tags.1"), 
                "CSV should contain expected headers");
            Assert.IsTrue(csvOutput.Contains("1,Laptop,Intel i7,16GB,electronics,computers"), 
                "CSV should contain expected data row");
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
            var parsedScript = JLioConvert.Parse(testCase.Script, parseOptions);
            var result = parsedScript.Execute(testCase.Data, executionContext);

            // Assert execution was successful
            if (!result.Success)
            {
                var errors = executionContext.Logger.LogEntries.Where(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error).ToList();
                var warnings = executionContext.Logger.LogEntries.Where(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Warning).ToList();
                
                Console.WriteLine($"Execution errors: {string.Join("; ", errors.Select(e => e.Message))}");
                Console.WriteLine($"Execution warnings: {string.Join("; ", warnings.Select(e => e.Message))}");
            }
            
            Assert.IsTrue(result.Success, $"Script execution failed");
            
            // Verify no errors in execution context
            var allErrors = executionContext.Logger.LogEntries.Where(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error).ToList();
            Assert.IsEmpty(allErrors, $"Execution errors found: {string.Join("; ", allErrors.Select(e => e.Message))}");

            // Compare results using deep equality (excluding dynamic fields like timestamp)
            var actualResult = result.Data;
            var areEqual = CompareResultsIgnoringDynamicFields(testCase.Expected, actualResult);
            
            if (!areEqual)
            {
                Console.WriteLine($"Expected:");
                Console.WriteLine(testCase.Expected.ToString());
                Console.WriteLine($"Actual:");
                Console.WriteLine(actualResult.ToString());
            }
            
            Assert.IsTrue(areEqual, $"Results do not match expected output");
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

        [Test]
        public void CanValidateToCsvCommand()
        {
            var toCsvCommand = new ToCsv
            {
                Path = "", // Empty path to trigger validation error
                CsvSettings = null // Null settings to trigger validation error
            };

            var validationResult = toCsvCommand.ValidateCommandInstance();

            Assert.IsFalse(validationResult.IsValid);
            Assert.Contains("Path property is required for toCsv command", validationResult.ValidationMessages);
            Assert.Contains("CsvSettings property is required for toCsv command", validationResult.ValidationMessages);
        }

        [Test]
        public void ToCsvCommand_InvalidSettings_ShouldFailValidation()
        {
            var toCsvCommand = new ToCsv
            {
                Path = "$.data",
                CsvSettings = new CsvSettings
                {
                    Delimiter = "", // Empty delimiter
                    EscapeQuoteChar = "", // Empty escape char
                    BooleanFormat = "true" // Missing comma
                }
            };

            var validationResult = toCsvCommand.ValidateCommandInstance();

            Assert.IsFalse(validationResult.IsValid);
            Assert.Contains("Delimiter cannot be empty in CsvSettings", validationResult.ValidationMessages);
            Assert.Contains("EscapeQuoteChar cannot be empty in CsvSettings", validationResult.ValidationMessages);
            Assert.Contains("BooleanFormat must contain comma-separated true,false values", validationResult.ValidationMessages);
        }

        private class TestCaseData
        {
            public JToken Data { get; set; }
            public string Script { get; set; }
            public JToken Expected { get; set; }
        }
    }
}