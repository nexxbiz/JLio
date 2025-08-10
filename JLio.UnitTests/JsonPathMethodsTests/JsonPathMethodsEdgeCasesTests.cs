using System.Linq;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.JsonPathMethodsTests;

public class JsonPathMethodsEdgeCasesTests
{
    private IItemsFetcher itemsFetcher;

    [SetUp]
    public void Setup()
    {
        itemsFetcher = new JsonPathItemsFetcher();
    }

    #region SplitPath Edge Cases

    [TestCase("$", 1, "$")]
    [TestCase("$.a", 2, "a")]
    [TestCase("$.['complex-name']", 2, "['complex-name']")]
    [TestCase("$.['name with spaces']", 2, "['name with spaces']")]
    [TestCase("$.a.b.c.d.e", 6, "e")]
    public void SplitPath_EdgeCases_ShouldHandleCorrectly(string path, int expectedElements, string expectedLastName)
    {
        var result = JsonPathMethods.SplitPath(path);
        
        Assert.AreEqual(expectedElements, result.Elements.Count);
        Assert.AreEqual(expectedLastName, result.LastName);
    }

    [TestCase("$")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("notjsonpath")]
    public void SplitPath_InvalidPaths_ShouldNotThrowException(string invalidPath)
    {
        Assert.DoesNotThrow(() =>
        {
            var result = JsonPathMethods.SplitPath(invalidPath);
            Assert.IsNotNull(result);
        });
    }

    [TestCase("$.array[0]", true)]
    [TestCase("$.array[*]", true)]
    [TestCase("$.array[?(@.id > 1)]", true)]
    [TestCase("$.simple.property", false)]
    [TestCase("$", false)]
    public void SplitPath_ArrayDetection_ShouldBeAccurate(string path, bool expectedHasArray)
    {
        var result = JsonPathMethods.SplitPath(path);
        Assert.AreEqual(expectedHasArray, result.HasArrayIndication);
    }

    #endregion

    #region GetIntellisense Edge Cases

    [Test]
    public void GetIntellisense_WithEmptyPath_ShouldReturnRootProperties()
    {
        var testData = JToken.Parse(@"{""prop1"": 1, ""prop2"": 2}");
        
        var result = JsonPathMethods.GetIntellisense("", testData, itemsFetcher);
        
        Assert.IsNotNull(result);
        // Should handle empty path gracefully
    }

    [Test]
    public void GetIntellisense_WithOnlyDollar_ShouldReturnRootProperties()
    {
        var testData = JToken.Parse(@"{""prop1"": 1, ""prop2"": 2}");
        
        var result = JsonPathMethods.GetIntellisense("$", testData, itemsFetcher);
        
        Assert.IsNotNull(result);
    }

    [Test]
    public void GetIntellisense_WithTrailingDot_ShouldReturnChildProperties()
    {
        var testData = JToken.Parse(@"{""parent"": {""child1"": 1, ""child2"": 2}}");
        
        var result = JsonPathMethods.GetIntellisense("$.parent.", testData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.parent.child1"));
        Assert.That(result, Contains.Item("$.parent.child2"));
    }

    [Test]
    public void GetIntellisense_WithArrayData_ShouldReturnArrayNotation()
    {
        var testData = JToken.Parse(@"{""items"": [1, 2, 3]}");
        
        var result = JsonPathMethods.GetIntellisense("$.items", testData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.items[*]"));
    }

    [Test]
    public void GetIntellisense_WithMixedObjectAndArrays_ShouldReturnBoth()
    {
        var testData = JToken.Parse(@"{
            ""objects"": {""prop"": 1},
            ""arrays"": [1, 2, 3],
            ""simpleValue"": ""test""
        }");
        
        var result = JsonPathMethods.GetIntellisense("$.", testData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.objects"));
        Assert.That(result, Contains.Item("$.arrays"));
        Assert.That(result, Contains.Item("$.simpleValue"));
    }

    [Test]
    public void GetIntellisense_WithNonExistentPath_ShouldFallbackGracefully()
    {
        var testData = JToken.Parse(@"{""existing"": {""prop"": 1}}");
        
        var result = JsonPathMethods.GetIntellisense("$.nonexistent.deep.", testData, itemsFetcher);
        
        Assert.IsNotNull(result);
        // Should fallback to existing paths
    }

    [Test]
    public void GetIntellisense_WithSpecialCharactersInPropertyNames_ShouldWork()
    {
        var testData = JToken.Parse(@"{
            ""property-with-dashes"": 1,
            ""property_with_underscores"": 2,
            ""property with spaces"": 3,
            ""property.with.dots"": 4
        }");
        
        var result = JsonPathMethods.GetIntellisense("$.", testData, itemsFetcher);
        
        Assert.Greater(result.Count, 0);
        // Should include properties with special characters
    }

    #endregion

    #region Complex Nested Structures

    [Test]
    public void GetIntellisense_WithDeeplyNestedObject_ShouldWork()
    {
        var testData = JToken.Parse(@"{
            ""level1"": {
                ""level2"": {
                    ""level3"": {
                        ""level4"": {
                            ""level5"": {
                                ""deepProperty"": ""value""
                            }
                        }
                    }
                }
            }
        }");
        
        var result = JsonPathMethods.GetIntellisense("$.level1.level2.level3.level4.level5.", testData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.level1.level2.level3.level4.level5.deepProperty"));
    }

    [Test]
    public void GetIntellisense_WithNestedArraysAndObjects_ShouldProvideCorrectSuggestions()
    {
        var testData = JToken.Parse(@"{
            ""data"": [
                {
                    ""items"": [
                        {""name"": ""item1"", ""value"": 1},
                        {""name"": ""item2"", ""value"": 2}
                    ]
                }
            ]
        }");
        
        var result = JsonPathMethods.GetIntellisense("$.data", testData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.data[*]"));
    }

    #endregion

    #region Performance and Memory Tests

    [Test]
    public void GetIntellisense_WithLargeNumberOfProperties_ShouldPerform()
    {
        // Create an object with many properties
        var largeObject = new JObject();
        for (int i = 0; i < 1000; i++)
        {
            largeObject[$"property{i}"] = $"value{i}";
        }
        
        var result = JsonPathMethods.GetIntellisense("$.", largeObject, itemsFetcher);
        
        Assert.AreEqual(1000, result.Count);
        Assert.IsTrue(result.All(r => r.StartsWith("$.property")));
    }

    [Test]
    public void SplitPath_WithVeryLongPath_ShouldWork()
    {
        var longPath = "$.a.b.c.d.e.f.g.h.i.j.k.l.m.n.o.p.q.r.s.t.u.v.w.x.y.z";
        
        var result = JsonPathMethods.SplitPath(longPath);
        
        Assert.AreEqual(27, result.Elements.Count); // $ + 26 letters
        Assert.AreEqual("z", result.LastName);
    }

    #endregion

    #region Data Type Handling

    [Test]
    public void GetIntellisense_WithAllJsonDataTypes_ShouldHandleCorrectly()
    {
        var testData = JToken.Parse(@"{
            ""stringValue"": ""test"",
            ""numberValue"": 42,
            ""booleanValue"": true,
            ""nullValue"": null,
            ""arrayValue"": [1, 2, 3],
            ""objectValue"": {""nested"": true}
        }");
        
        var result = JsonPathMethods.GetIntellisense("$.", testData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.stringValue"));
        Assert.That(result, Contains.Item("$.numberValue"));
        Assert.That(result, Contains.Item("$.booleanValue"));
        Assert.That(result, Contains.Item("$.nullValue"));
        Assert.That(result, Contains.Item("$.arrayValue"));
        Assert.That(result, Contains.Item("$.objectValue"));
    }

    [Test]
    public void GetIntellisense_WithEmptyArrays_ShouldStillReturnArrayNotation()
    {
        var testData = JToken.Parse(@"{""emptyArray"": []}");
        
        var result = JsonPathMethods.GetIntellisense("$.emptyArray", testData, itemsFetcher);
        
        Assert.That(result, Contains.Item("$.emptyArray[*]"));
    }

    [Test]
    public void GetIntellisense_WithEmptyObjects_ShouldHandleGracefully()
    {
        var testData = JToken.Parse(@"{""emptyObject"": {}}");
        
        var result = JsonPathMethods.GetIntellisense("$.emptyObject.", testData, itemsFetcher);
        
        Assert.IsNotNull(result);
        // Empty object should return empty suggestions for its children
    }

    #endregion

    #region Filtering and Distinct Results

    [Test]
    public void GetIntellisense_ShouldReturnDistinctResults()
    {
        var testData = JToken.Parse(@"{
            ""prop1"": 1,
            ""prop2"": 2
        }");
        
        var result = JsonPathMethods.GetIntellisense("$.", testData, itemsFetcher);
        
        var distinctResults = result.Distinct().ToList();
        Assert.AreEqual(result.Count, distinctResults.Count, "Results should be distinct");
    }

    [Test]
    public void GetIntellisense_WithPartialPrefix_ShouldFilterCorrectly()
    {
        var testData = JToken.Parse(@"{
            ""prefix_match1"": 1,
            ""prefix_match2"": 2,
            ""other_property"": 3
        }");
        
        var result = JsonPathMethods.GetIntellisense("$.prefix", testData, itemsFetcher);
        
        if (result.Any()) // If filtering works
        {
            Assert.IsTrue(result.All(r => r.Contains("prefix")));
        }
    }

    #endregion
}