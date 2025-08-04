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
using JLio.Extensions.Text;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests
{
    public class ResolveTests
    {
        private IExecutionContext executionContext;
        private ParseOptions parseOptions;
        private string testDataPath;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            parseOptions.RegisterETL().RegisterText();
            executionContext = ExecutionContext.CreateDefault();
            
            // Get the test data directory path
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            testDataPath = Path.Combine(assemblyDirectory, "TestData", "ResolveTests");
        }


  

        [TestCase("simple-key-to-array", TestName = "Simple Key to Array Matching")]
        [TestCase("multiple-items-array", TestName = "Multiple Items Array Matching")]
        [TestCase("array-to-array-intersection", TestName = "Array to Array Intersection")]
        [TestCase("multiple-array-matches", TestName = "Multiple Array Matches Edge Case")]
        [TestCase("user-permission-resolution", TestName = "Basic User-Permission Resolution")]
        [TestCase("multiple-collections", TestName = "Multiple Collections Resolution")]
        [TestCase("array-reference-matching", TestName = "Array Reference Matching with Multiple Collections")]
        [TestCase("no-array-matches", TestName = "No Array Matches Edge Case")]
        [TestCase("empty-reference-arrays", TestName = "Empty Reference Arrays")]
        [TestCase("empty-collections", TestName = "Empty Collections")]
        [TestCase("partial-matches", TestName = "Partial Matches Across Collections")]
        [TestCase("nested-subcollections", TestName = "Nested Subcollections Cross-Reference Resolution")]
        [TestCase("asarray-behavior", TestName = "AsArray Behavior Options")]
        public void ResolveFileBasedTests(string testCaseName)
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


        [Test]
        public void CanMatchSimpleKeyToArrayValues()
        {
            // Test Case 1: Simple Key to Array Matching - Exact user scenario from design doc
            var testData = JToken.Parse(@"{
                ""items"": [{""refKey"": 1, ""name"": ""Item One""}],
                ""references"": [{""a"": 1, ""keys"": [1, 2], ""category"": ""Group A""}]
            }");

            var functionConverter = new FunctionConverter(parseOptions.FunctionsProvider);

            var resolveCommand = new Resolve
            {
                Path = "$.items[*]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.refKey", ReferenceKeyPath = "@.keys[*]" }
                        },
                        ReferencesCollectionPath = "$.references[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue { TargetPath = "@.referenceData", Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""), functionConverter)) }
                        }
                    }
                }
            };

            var result = resolveCommand.Execute(testData, executionContext);

            Assert.IsTrue(result.Success);
            
            // Check that the basic matching and reference data assignment works
            Assert.IsNotNull(testData.SelectToken("$.items[0].referenceData"));
            Assert.AreEqual(1, testData.SelectToken("$.items[0].referenceData.a")?.Value<int>());
            Assert.AreEqual("Group A", testData.SelectToken("$.items[0].referenceData.category")?.Value<string>());
        }

        [Test]
        public void CanMatchMultipleItemsWithArrayMatching()
        {
            // Test Case 2: Multiple Items with Array Matching
            var testData = JToken.Parse(@"{
                ""items"": [
                    {""refKey"": 1, ""name"": ""Item One""},
                    {""refKey"": 3, ""name"": ""Item Three""},
                    {""refKey"": 5, ""name"": ""Item Five""}
                ],
                ""references"": [
                    {""id"": ""REF001"", ""keys"": [1, 2], ""category"": ""Group A""},
                    {""id"": ""REF002"", ""keys"": [3, 4], ""category"": ""Group B""},
                    {""id"": ""REF003"", ""keys"": [5, 6, 7], ""category"": ""Group C""}
                ]
            }");

            var resolveCommand = new Resolve
            {
                Path = "$.items[*]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.refKey", ReferenceKeyPath = "@.keys[*]" }
                        },
                        ReferencesCollectionPath = "$.references[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue { TargetPath = "@.referenceData", Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))) },
                            new ResolveValue { TargetPath = "@.category", Value = new FunctionSupportedValue(new Fetch("@.category")) },
                            new ResolveValue { TargetPath = "@.referenceId", Value = new FunctionSupportedValue(new Fetch("@.id")) }
                        }
                    }
                }
            };

            var result = resolveCommand.Execute(testData, executionContext);

            Assert.IsTrue(result.Success);

            // Verify Item One
            Assert.AreEqual("Group A", testData.SelectToken("$.items[0].category")?.Value<string>());
            Assert.AreEqual("REF001", testData.SelectToken("$.items[0].referenceId")?.Value<string>());

            // Verify Item Three
            Assert.AreEqual("Group B", testData.SelectToken("$.items[1].category")?.Value<string>());
            Assert.AreEqual("REF002", testData.SelectToken("$.items[1].referenceId")?.Value<string>());

            // Verify Item Five
            Assert.AreEqual("Group C", testData.SelectToken("$.items[2].category")?.Value<string>());
            Assert.AreEqual("REF003", testData.SelectToken("$.items[2].referenceId")?.Value<string>());
        }

        [Test]
        public void CanHandleMultipleArrayMatches()
        {
            // Test Case 4: Multiple Array Matches Edge Case
            var testData = JToken.Parse(@"{
                ""items"": [{""refKey"": 1, ""name"": ""Popular Item""}],
                ""references"": [
                    {""id"": ""REF001"", ""keys"": [1, 2], ""category"": ""Group A""},
                    {""id"": ""REF002"", ""keys"": [1, 3], ""category"": ""Group B""}
                ]
            }");

            var resolveCommand = new Resolve
            {
                Path = "$.items[*]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.refKey", ReferenceKeyPath = "@.keys[*]" }
                        },
                        ReferencesCollectionPath = "$.references[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue { TargetPath = "@.referenceData", Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))) },
                            new ResolveValue { TargetPath = "@.categories", Value = new FunctionSupportedValue(new Fetch("@.category")) }
                        }
                    }
                }
            };

            var result = resolveCommand.Execute(testData, executionContext);

            Assert.IsTrue(result.Success);

            // Should return array for multiple matches
            var referenceData = testData.SelectToken("$.items[0].referenceData");
            Assert.IsTrue(referenceData is JArray, "Should return array for multiple matches");
            Assert.AreEqual(2, ((JArray)referenceData).Count, "Should have 2 matching references");

            var categories = testData.SelectToken("$.items[0].categories") as JArray;
            Assert.IsNotNull(categories);
            Assert.AreEqual(2, categories.Count);
            Assert.Contains("Group A", categories.Select(t => t.Value<string>()).ToArray());
            Assert.Contains("Group B", categories.Select(t => t.Value<string>()).ToArray());
        }

        [Test]
        public void CanResolveBasicUserPermissions()
        {
            // Test Case 5: Basic User-Permission Resolution
            var testData = JToken.Parse(@"{
                ""users"": [
                    {""id"": ""USR100"", ""name"": ""Alice""},
                    {""id"": ""USR101"", ""name"": ""Bob""}
                ],
                ""permissions"": [
                    {""userId"": ""USR100"", ""role"": ""admin"", ""scope"": ""global""},
                    {""userId"": ""USR101"", ""role"": ""viewer"", ""scope"": ""limited""}
                ]
            }");

            var resolveCommand = new Resolve
            {
                Path = "$.users[*]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.id", ReferenceKeyPath = "@.userId" }
                        },
                        ReferencesCollectionPath = "$.permissions[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue { TargetPath = "@.fullPermission", Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))) },
                            new ResolveValue { TargetPath = "@.role", Value = new FunctionSupportedValue(new Fetch("@.role")) }
                        }
                    }
                }
            };

            var result = resolveCommand.Execute(testData, executionContext);

            Assert.IsTrue(result.Success);

            // Verify Alice
            Assert.AreEqual("admin", testData.SelectToken("$.users[0].role")?.Value<string>());
            Assert.IsNotNull(testData.SelectToken("$.users[0].fullPermission"));

            // Verify Bob
            Assert.AreEqual("viewer", testData.SelectToken("$.users[1].role")?.Value<string>());
            Assert.IsNotNull(testData.SelectToken("$.users[1].fullPermission"));
        }

        [Test]
        public void CanHandleNoMatches()
        {
            // Test Case 8: Edge Cases - No Array Matches
            var testData = JToken.Parse(@"{
                ""items"": [{""refKey"": 99, ""name"": ""Orphaned Item""}],
                ""references"": [
                    {""id"": ""REF001"", ""keys"": [1, 2], ""category"": ""Group A""},
                    {""id"": ""REF002"", ""keys"": [3, 4], ""category"": ""Group B""}
                ]
            }");

            var resolveCommand = new Resolve
            {
                Path = "$.items[*]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.refKey", ReferenceKeyPath = "@.keys[*]" }
                        },
                        ReferencesCollectionPath = "$.references",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue { TargetPath = "@.referenceData", Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))) },
                            new ResolveValue { TargetPath = "@.category", Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"=fetch(@.category)\""))) }
                        }
                    }
                }
            };

            var result = resolveCommand.Execute(testData, executionContext);

            Assert.IsTrue(result.Success);

            // Items should remain unchanged, no reference data added
            Assert.AreEqual("Orphaned Item", testData.SelectToken("$.items[0].name")?.Value<string>());
            Assert.AreEqual(99, testData.SelectToken("$.items[0].refKey")?.Value<int>());
            Assert.IsNull(testData.SelectToken("$.items[0].referenceData"));
            Assert.IsNull(testData.SelectToken("$.items[0].category"));
        }

        [Test]
        public void CanValidateCommandInstance()
        {
            var resolveCommand = new Resolve
            {
                Path = "" // Empty path to trigger validation error
            };

            var validationResult = resolveCommand.ValidateCommandInstance();

            Assert.IsFalse(validationResult.IsValid);
            Assert.Contains("Path property for resolve command is missing", validationResult.ValidationMessages);
            Assert.Contains("ResolveSettings property for resolve command is missing or empty", validationResult.ValidationMessages);
        }

        [Test]
        public void CanValidateResolveSettings()
        {
            var resolveCommand = new Resolve
            {
                Path = "$.items[*]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ReferencesCollectionPath = "" // Empty to trigger validation error
                        // Missing ResolveKeys and Values to trigger those validation errors
                    }
                }
            };

            var validationResult = resolveCommand.ValidateCommandInstance();

            Assert.IsFalse(validationResult.IsValid);
            Assert.Contains("ResolveKeys are required for each resolve setting", validationResult.ValidationMessages);
            Assert.Contains("ReferencesCollectionPath is required for each resolve setting", validationResult.ValidationMessages);
            Assert.Contains("Values are required for each resolve setting", validationResult.ValidationMessages);
        }

        [Test]
        public void CanHandleJsonScriptParsing()
        {
            // Simple JSON script test
            var script = @"[{
                ""path"": ""$.items[*]"",
                ""command"": ""resolve"",
                ""resolveSettings"": [
                    {
                        ""resolveKeys"": [{""keyPath"": ""@.refKey"", ""referenceKeyPath"": ""@.keys[*]""}],
                        ""referencesCollectionPath"": ""$.references[*]"",
                        ""values"": [
                            {""targetPath"": ""@.referenceData"", ""value"": ""@""},
                            {""targetPath"": ""@.category"", ""value"": ""=fetch(@.category)""}
                        ]
                    }
                ]
            }]";

            var testData = JToken.Parse(@"{
                ""items"": [{""refKey"": 1, ""name"": ""Item One""}],
                ""references"": [{""a"": 1, ""keys"": [1, 2], ""category"": ""Group A""}]
            }");

            var parsedScript = JLioConvert.Parse(script, parseOptions);
            var result = parsedScript.Execute(testData, executionContext);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Group A", testData.SelectToken("$.items[0].category")?.Value<string>());
            Assert.IsNotNull(testData.SelectToken("$.items[0].referenceData"));
        }


        [Test]
        public void CanUseAsArrayBehaviorOptions()
        {
            // Test Case: AsArray Behavior Options
            // This test demonstrates all three AsArray behavior options
            var testData = JToken.Parse(@"{
                ""items"": [
                    {""id"": ""ITEM001"", ""categoryId"": ""CAT001""},
                    {""id"": ""ITEM002"", ""categoryId"": ""CAT999""},
                    {""id"": ""ITEM003"", ""categoryIds"": [""CAT001"", ""CAT002""]}
                ],
                ""categories"": [
                    {""id"": ""CAT001"", ""name"": ""Electronics""},
                    {""id"": ""CAT002"", ""name"": ""Mobile""}
                ]
            }");

            // Test AlwaysAsArray - single match should return array with one element
            var resolveAlwaysArrayCommand = new Resolve
            {
                Path = "$.items[0]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.categoryId", ReferenceKeyPath = "@.id" }
                        },
                        ReferencesCollectionPath = "$.categories[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue 
                            { 
                                TargetPath = "@.alwaysArrayResult", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.AlwaysAsArray
                            }
                        }
                    }
                }
            };

            var result1 = resolveAlwaysArrayCommand.Execute(testData, executionContext);
            Assert.IsTrue(result1.Success);

            var alwaysArrayResult = testData.SelectToken("$.items[0].alwaysArrayResult") as JArray;
            Assert.IsNotNull(alwaysArrayResult, "AlwaysAsArray should return JArray even for single match");
            Assert.AreEqual(1, alwaysArrayResult.Count, "AlwaysAsArray should have 1 element for single match");
            Assert.AreEqual("Electronics", alwaysArrayResult[0].SelectToken("name")?.Value<string>());

            // Test AlwaysAsArray - no matches should return empty array
            var resolveNoMatchArrayCommand = new Resolve
            {
                Path = "$.items[1]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.categoryId", ReferenceKeyPath = "@.id" }
                        },
                        ReferencesCollectionPath = "$.categories[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue 
                            { 
                                TargetPath = "@.noMatchArrayResult", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.AlwaysAsArray
                            }
                        }
                    }
                }
            };

            var result2 = resolveNoMatchArrayCommand.Execute(testData, executionContext);
            Assert.IsTrue(result2.Success);

            var noMatchArrayResult = testData.SelectToken("$.items[1].noMatchArrayResult") as JArray;
            Assert.IsNotNull(noMatchArrayResult, "AlwaysAsArray should return empty JArray for no matches");
            Assert.AreEqual(0, noMatchArrayResult.Count, "AlwaysAsArray should have 0 elements for no matches");

            // Test AlwaysAsObject - single match should return object
            var resolveAlwaysObjectCommand = new Resolve
            {
                Path = "$.items[0]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.categoryId", ReferenceKeyPath = "@.id" }
                        },
                        ReferencesCollectionPath = "$.categories[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue 
                            { 
                                TargetPath = "@.alwaysObjectResult", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.AlwaysAsObject
                            }
                        }
                    }
                }
            };

            var result3 = resolveAlwaysObjectCommand.Execute(testData, executionContext);
            Assert.IsTrue(result3.Success);

            var alwaysObjectResult = testData.SelectToken("$.items[0].alwaysObjectResult");
            Assert.IsTrue(alwaysObjectResult is JObject, "AlwaysAsObject should return JObject for single match");
            Assert.AreEqual("Electronics", alwaysObjectResult.SelectToken("name")?.Value<string>());

            // Test DependingOnResult (default) - single match should return object
            var resolveDependingCommand = new Resolve
            {
                Path = "$.items[0]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.categoryId", ReferenceKeyPath = "@.id" }
                        },
                        ReferencesCollectionPath = "$.categories[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue 
                            { 
                                TargetPath = "@.dependingResult", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.DependingOnResult
                            }
                        }
                    }
                }
            };

            var result4 = resolveDependingCommand.Execute(testData, executionContext);
            Assert.IsTrue(result4.Success);

            var dependingResult = testData.SelectToken("$.items[0].dependingResult");
            Assert.IsTrue(dependingResult is JObject, "DependingOnResult should return JObject for single match");
            Assert.AreEqual("Electronics", dependingResult.SelectToken("name")?.Value<string>());

            // Test multiple matches with DependingOnResult - should return array
            var resolveMultipleDependingCommand = new Resolve
            {
                Path = "$.items[2]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.categoryIds[*]", ReferenceKeyPath = "@.id" }
                        },
                        ReferencesCollectionPath = "$.categories[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue 
                            { 
                                TargetPath = "@.multipleDependingResult", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.DependingOnResult
                            }
                        }
                    }
                }
            };

            var result5 = resolveMultipleDependingCommand.Execute(testData, executionContext);
            Assert.IsTrue(result5.Success);

            var multipleDependingResult = testData.SelectToken("$.items[2].multipleDependingResult") as JArray;
            Assert.IsNotNull(multipleDependingResult, "DependingOnResult should return JArray for multiple matches");
            Assert.AreEqual(2, multipleDependingResult.Count, "DependingOnResult should have 2 elements for multiple matches");
        }

        [Test]
        public void CanHandleAlwaysAsObjectWithMultipleMatchesError()
        {
            // Test Case: AlwaysAsObject with multiple matches should throw error
            var testData = JToken.Parse(@"{
                ""items"": [
                    {""id"": ""ITEM003"", ""categoryIds"": [""CAT001"", ""CAT002""]}
                ],
                ""categories"": [
                    {""id"": ""CAT001"", ""name"": ""Electronics""},
                    {""id"": ""CAT002"", ""name"": ""Mobile""}
                ]
            }");

            var resolveAlwaysObjectMultipleCommand = new Resolve
            {
                Path = "$.items[0]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.categoryIds[*]", ReferenceKeyPath = "@.id" }
                        },
                        ReferencesCollectionPath = "$.categories[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue 
                            { 
                                TargetPath = "@.errorResult", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.AlwaysAsObject
                            }
                        }
                    }
                }
            };

            // Should throw InvalidOperationException for multiple matches with AlwaysAsObject

                var result = resolveAlwaysObjectMultipleCommand.Execute(testData, executionContext);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void CanTestNewAsArrayBehaviorQuickly()
        {
            // Quick test to verify the new AsArray behavior works
            var testData = JToken.Parse(@"{
                ""items"": [{""categoryId"": ""CAT001""}],
                ""categories"": [{""id"": ""CAT001"", ""name"": ""Electronics""}]
            }");

            var resolveCommand = new Resolve
            {
                Path = "$.items[*]",
                ResolveSettings = new List<ResolveSetting>
                {
                    new ResolveSetting
                    {
                        ResolveKeys = new List<ResolveKey>
                        {
                            new ResolveKey { KeyPath = "@.categoryId", ReferenceKeyPath = "@.id" }
                        },
                        ReferencesCollectionPath = "$.categories[*]",
                        Values = new List<ResolveValue>
                        {
                            new ResolveValue 
                            { 
                                TargetPath = "@.alwaysArray", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.AlwaysAsArray
                            },
                            new ResolveValue 
                            { 
                                TargetPath = "@.dependingResult", 
                                Value = new FunctionSupportedValue(new FixedValue(JToken.Parse("\"@\""))),
                                ResolveTypeBehavior = ResolveTypeBehavior.DependingOnResult
                            }
                        }
                    }
                }
            };

            var result = resolveCommand.Execute(testData, executionContext);
            Assert.IsTrue(result.Success);

            // Check AlwaysAsArray - should be array even for single match
            var alwaysArrayResult = testData.SelectToken("$.items[0].alwaysArray");
            Assert.IsTrue(alwaysArrayResult is JArray, "AlwaysAsArray should return JArray");
            Assert.AreEqual(1, ((JArray)alwaysArrayResult).Count);

            // Check DependingOnResult - should be object for single match
            var dependingResult = testData.SelectToken("$.items[0].dependingResult");
            Assert.IsTrue(dependingResult is JObject, "DependingOnResult should return JObject for single match");
            
            Console.WriteLine($"Test successful - AlwaysArray type: {alwaysArrayResult.Type}, DependingResult type: {dependingResult.Type}");
        }
    }
}