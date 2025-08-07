using System.Collections.Generic;
using System.Linq;
using JLio.Core.Extensions;
using JLio.Core;
using JLio.Core.Contracts;  
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.JTokenTests;

public class JTokenTests
{
    private IExecutionContext executionContext;

    [SetUp]
    public void Setup()
    {
        executionContext = ExecutionContext.CreateDefault();
    }

    [Test]
    public void ConvertFromDictionary()
    {
        var source = new Dictionary<string, JToken>
        {
            {"item1", new JValue(1)},
            {"item2", new JValue(2)}
        };
        var sut = source.ConvertToDataObject();
        Assert.AreEqual(1, sut.SelectToken("$.item1")?.Value<int>());
        Assert.AreEqual(2, sut.SelectToken("$.item2")?.Value<int>());
    }

    [Test]
    public void ConvertToDictionary()
    {
        var source = JObject.Parse("{\"item1\":1,\"item2\":2}");
        var sut = source.ConvertToDictionary();
        Assert.IsTrue(sut.ContainsKey("item1"));
        Assert.IsTrue(sut.ContainsKey("item2"));
        Assert.AreEqual(1, sut["item1"].Value<int>());
        Assert.AreEqual(2, sut["item2"].Value<int>());
    }

    [Test]
    public void ConvertToDictionaryWithFilter()
    {
        var source = JObject.Parse("{\"item1\":1,\"item2\":2}");
        var sut = source.ConvertToDictionary(new List<string> {"item1"});
        Assert.IsTrue(sut.ContainsKey("item1"));
        Assert.IsFalse(sut.ContainsKey("item2"));
        Assert.AreEqual(1, sut["item1"].Value<int>());
    }

    [TestCase("$.item[*]", "{\"item\":[1,\"a\"]}", false)]
    [TestCase("$.item[*]", "{\"item\":[1,2]}", true)]
    public void SelectSameTokenTypes(string path, string jObject, bool sameTypes)
    {
        var source = JObject.Parse(jObject);
        var sut = new JsonPathItemsFetcher().SelectTokens("$.item[*]", source);

        Assert.AreEqual(sameTypes, sut.AreSameTokenTypes);
    }

    [TestCase("$.item[*]", "{\"item\":[1,\"a\"]}", JTokenType.Integer, 1)]
    [TestCase("$.item[*]", "{\"item\":[1,2]}", JTokenType.Integer, 2)]
    public void SelectByJTokenType(string path, string jObject, JTokenType type, int numberOfItems)
    {
        var source = JObject.Parse(jObject);
        var sut = new JsonPathItemsFetcher().SelectTokens("$.item[*]", source);

        Assert.AreEqual(numberOfItems, sut.GetTokens(type).Count());
    }

    #region Parent Navigation Tests

    [TestCase("@.<--.value", "{\"parent\":{\"value\":\"parentValue\",\"child\":{\"name\":\"childName\"}}}", "parentValue")]
    [TestCase("@.<--.name", "{\"parent\":{\"name\":\"parentName\",\"child\":{\"value\":\"childValue\"}}}", "parentName")]
    public void ParentNavigation_SingleLevel_ReturnsParentValue(string path, string json, string expectedValue)
    {
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.parent.child");
        
        var fixedValue = new FixedValue(JValue.CreateString(path));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedValue, result.Data.First().Value<string>());
    }

    [TestCase("@.<--.<--.value", "{\"grandparent\":{\"value\":\"grandparentValue\",\"parent\":{\"child\":{\"name\":\"childName\"}}}}", "grandparentValue")]
    [TestCase("@.<--.<--.name", "{\"grandparent\":{\"name\":\"grandparentName\",\"parent\":{\"child\":{\"value\":\"childValue\"}}}}", "grandparentName")]
    public void ParentNavigation_MultipleLevel_ReturnsGrandparentValue(string path, string json, string expectedValue)
    {
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.grandparent.parent.child");
        
        var fixedValue = new FixedValue(JValue.CreateString(path));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedValue, result.Data.First().Value<string>());
    }

    [TestCase("@.<--.items[0].name", "{\"parent\":{\"items\":[{\"name\":\"item1\"},{\"name\":\"item2\"}],\"child\":{\"value\":\"test\"}}}", "item1")]
    [TestCase("@.<--.items[1].price", "{\"parent\":{\"items\":[{\"name\":\"item1\",\"price\":10},{\"name\":\"item2\",\"price\":20}],\"child\":{\"value\":\"test\"}}}", 20)]
    public void ParentNavigation_ComplexPath_ReturnsCorrectValue(string path, string json, object expectedValue)
    {
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.parent.child");
        
        var fixedValue = new FixedValue(JValue.CreateString(path));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        if (expectedValue is string)
            Assert.AreEqual(expectedValue, result.Data.First().Value<string>());
        else if (expectedValue is int)
            Assert.AreEqual(expectedValue, result.Data.First().Value<int>());
    }

    [Test]
    public void ParentNavigation_InArray_ReturnsParentArrayElement()
    {
        var json = @"{
            ""orders"": [
                {
                    ""id"": 1,
                    ""items"": [
                        {""name"": ""product1"", ""quantity"": 2},
                        {""name"": ""product2"", ""quantity"": 3}
                    ]
                },
                {
                    ""id"": 2,
                    ""items"": [
                        {""name"": ""product3"", ""quantity"": 1}
                    ]
                }
            ]
        }";
        
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.orders[0].items[1]"); // product2
        
        var fixedValue = new FixedValue(JValue.CreateString("@.<--.id"));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Data.First().Value<int>());
    }

    [Test]
    public void ParentNavigation_NoParent_ReturnsOriginalValue()
    {
        var json = @"{""value"": ""rootValue""}";
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext; // Root has no parent
        
        var fixedValue = new FixedValue(JValue.CreateString("@.<--.someProperty"));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        // Should return the original value when parent navigation fails
        Assert.IsTrue(JToken.DeepEquals(dataContext,result.Data.First()));
    }

    [Test]
    public void ParentNavigation_TooManyLevels_ReturnsOriginalValue()
    {
        var json = @"{""parent"":{""child"":{""value"":""test""}}}";
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.parent.child");
        
        // Try to go up 5 levels when only 2 exist
        var fixedValue = new FixedValue(JValue.CreateString("@.<--.<--.<--.<--.<--.nonExistent"));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        // Should return the original value when too many parent levels are requested
        Assert.IsTrue(JToken.DeepEquals(currentToken, result.Data.First()));
    }

    [Test]
    public void ParentNavigation_EndingWithParent_ReturnsParentToken()
    {
        var json = @"{""parent"":{""name"":""parentName"",""child"":{""value"":""childValue""}}}";
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.parent.child");
        
        var fixedValue = new FixedValue(JValue.CreateString("@.<--"));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        var parentToken = result.Data.First();
        Assert.AreEqual("parentName", parentToken.SelectToken("$.name").Value<string>());
    }

    [TestCase("@.<--.config.timeout", "{\"app\":{\"config\":{\"timeout\":30},\"module\":{\"setting\":\"value\"}}}", 30)]
    [TestCase("@.<--.config.retries", "{\"app\":{\"config\":{\"retries\":3},\"module\":{\"setting\":\"value\"}}}", 3)]
    public void ParentNavigation_NestedConfiguration_ReturnsCorrectValue(string path, string json, int expectedValue)
    {
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.app.module");
        
        var fixedValue = new FixedValue(JValue.CreateString(path));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(expectedValue, result.Data.First().Value<int>());
    }

    [Test]
    public void ParentNavigation_MixedWithComplexObject_ReturnsCorrectStructure()
    {
        var json = @"{
            ""company"": {
                ""name"": ""TechCorp"",
                ""departments"": [
                    {
                        ""name"": ""IT"",
                        ""employees"": [
                            {""name"": ""John"", ""role"": ""Developer""},
                            {""name"": ""Jane"", ""role"": ""Manager""}
                        ]
                    }
                ]
            }
        }";
        
        var dataContext = JObject.Parse(json);
        var currentToken = dataContext.SelectToken("$.company.departments[0].employees[0]");
        
        var fixedValue = new FixedValue(JValue.CreateString("@.<--.<--.name"));
        var result = fixedValue.Execute(currentToken, dataContext, executionContext);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual("TechCorp", result.Data.First().Value<string>());
    }

    #endregion
}