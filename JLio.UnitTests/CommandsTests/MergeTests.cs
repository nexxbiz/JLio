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

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
    }

    [Test]
    public void MergeTwoComplexArraysUnique()
    {
        var expectedResult =
            "{\"first\": [  {    \"item\": \"1\"  },  {    \"item\": 2  },  {    \"item\": 3.1  }],\"second\": [  {    \"item\": \"4\"  },  {    \"item\": 5  },  {    \"item\": 3.1  },  {    \"item\": \"1\"  },  {    \"item\": 2  }]}";
        var data =
            JObject.Parse(
                "{\"first\":[{\"item\":\"1\"},{\"item\":2},{\"item\":3.1}],\"second\":[{\"item\":\"4\"},{\"item\":5},{\"item\":3.1}]}");

        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
                {new MergeArraySettings {ArrayPath = "$.second", UniqueItemsWithoutKeys = true}}
        };

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexArraysUniqueWithComplexKeys()
    {
        var expectedResult =
            "{\"first\":[{\"key\":{\"id\":\"1\",\"demo\":3,\"sub\":\"a\"},\"item\":\"1a\",\"valueFirst\":\"first id 1a\",\"valueCommon\":\"common first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueFirst\":\"first id 2a\",\"valueCommon\":\"common first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueFirst\":\"first id 1b\",\"valueCommon\":\"common first id 1b\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}],\"second\":[{\"key\":{\"id\":\"1\",\"sub\":\"a\",\"demo\":3},\"item\":\"1a\",\"valueSecond\":\"second id 1a\",\"valueCommon\":\"common first id 1a\",\"valueFirst\":\"first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueSecond\":\"second id 2a\",\"valueCommon\":\"common first id 2a\",\"valueFirst\":\"first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueSecond\":\"second id 1b\",\"valueCommon\":\"common first id 1b\",\"valueFirst\":\"first id 1b\"},{\"key\":{\"id\":\"3\",\"sub\":\"a\"},\"item\":\"3.1\",\"valueSecond\":\"second id 3a\",\"valueCommon\":\"common scond id 3a\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}]}";
        var data = JObject.Parse(
            "{\"first\":[{\"key\":{\"id\":\"1\", \"demo\": 3 ,\"sub\":\"a\"},\"item\":\"1a\",\"valueFirst\":\"first id 1a\",\"valueCommon\":\"common first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueFirst\":\"first id 2a\",\"valueCommon\":\"common first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueFirst\":\"first id 1b\",\"valueCommon\":\"common first id 1b\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}],\"second\":[{\"key\":{\"id\":\"1\",\"sub\":\"a\"},\"item\":\"4\",\"valueSecond\":\"second id 1a\",\"valueCommon\":\"common scond id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":5,\"valueSecond\":\"second id 2a\",\"valueCommon\":\"common scond id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueSecond\":\"second id 1b\",\"valueCommon\":\"common scond id 1b\"},{\"key\":{\"id\":\"3\",\"sub\":\"a\"},\"item\":\"3.1\",\"valueSecond\":\"second id 3a\",\"valueCommon\":\"common scond id 3a\"}]}");


        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
            {
                new MergeArraySettings
                    {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id", "key.sub"}}
            }
        };

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);

        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexArraysUniqueWithoutComplexKeys()
    {
        var expectedResult =
            "{\"first\":[{\"key\":{\"id\":\"1\",\"demo\":3,\"sub\":\"a\"},\"item\":\"1a\",\"valueFirst\":\"first id 1a\",\"valueCommon\":\"common first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueFirst\":\"first id 2a\",\"valueCommon\":\"common first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueFirst\":\"first id 1b\",\"valueCommon\":\"common first id 1b\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}],\"second\":[{\"key\":{\"id\":\"1\",\"sub\":\"a\",\"demo\":3},\"item\":\"1a\",\"valueSecond\":\"second id 1a\",\"valueCommon\":\"common first id 1a\",\"valueFirst\":\"first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueSecond\":\"second id 2a\",\"valueCommon\":\"common first id 2a\",\"valueFirst\":\"first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueSecond\":\"second id 1b\",\"valueCommon\":\"common first id 1b\",\"valueFirst\":\"first id 1b\"},{\"key\":{\"id\":\"3\",\"sub\":\"a\"},\"item\":\"3.1\",\"valueSecond\":\"second id 3a\",\"valueCommon\":\"common scond id 3a\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}]}";
        var data = JObject.Parse(
            "{\"first\":[{\"key\":{\"id\":\"1\", \"demo\": 3 ,\"sub\":\"a\"},\"item\":\"1a\",\"valueFirst\":\"first id 1a\",\"valueCommon\":\"common first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueFirst\":\"first id 2a\",\"valueCommon\":\"common first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueFirst\":\"first id 1b\",\"valueCommon\":\"common first id 1b\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}],\"second\":[{\"key\":{\"id\":\"1\",\"sub\":\"a\"},\"item\":\"4\",\"valueSecond\":\"second id 1a\",\"valueCommon\":\"common scond id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":5,\"valueSecond\":\"second id 2a\",\"valueCommon\":\"common scond id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueSecond\":\"second id 1b\",\"valueCommon\":\"common scond id 1b\"},{\"key\":{\"id\":\"3\",\"sub\":\"a\"},\"item\":\"3.1\",\"valueSecond\":\"second id 3a\",\"valueCommon\":\"common scond id 3a\"}]}");


        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
            {
                new MergeArraySettings
                    {ArrayPath = "$.second"}
            }
        };

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);

        Assert.IsFalse(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeys()
    {
        var expectedResult =
            "{\"first\": [  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueFirst\": \"first id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueFirst\": \"first id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }],\"second\": [  {    \"key\": {      \"id\": \"4\"    },    \"item\": \"4\",    \"valueSecond\": \"second id 4\",    \"valueCommon\": \"common scond id 4\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueSecond\": \"second id 2\",    \"valueCommon\": \"common first id 2\",    \"valueFirst\": \"first id 2\"  },  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueSecond\": \"second id 1\",    \"valueCommon\": \"common first id 1\",    \"valueFirst\": \"first id 1\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }]}";
        var data = JObject.Parse(
            "{\"first\":[{\"key\":{\"id\":\"1\"},\"item\":\"1\",\"valueFirst\":\"first id 1\",\"valueCommon\":\"common first id 1\"},{\"key\":{\"id\":\"2\"},\"item\":2,\"valueFirst\":\"first id 2\",\"valueCommon\":\"common first id 2\"},{\"key\":{\"id\":\"3\"},\"item\":3.1,\"valueFirst\":\"first id 3\",\"valueCommon\":\"common first id 3\"}],\"second\":[{\"key\":{\"id\":\"4\"},\"item\":\"4\",\"valueSecond\":\"second id 4\",\"valueCommon\":\"common scond id 4\"},{\"key\":{\"id\":\"2\"},\"item\":5,\"valueSecond\":\"second id 2\",\"valueCommon\":\"common scond id 2\"},{\"key\":{\"id\":\"1\"},\"item\":3.1,\"valueSecond\":\"second id 1\",\"valueCommon\":\"common scond id 1\"}]}");

        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
                {new MergeArraySettings {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id"}}}
        };
        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexArraysUniqueWithoutKeys()
    {
        var expectedResult =
            "{\"first\": [  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueFirst\": \"first id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueFirst\": \"first id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }],\"second\": [  {    \"key\": {      \"id\": \"4\"    },    \"item\": \"4\",    \"valueSecond\": \"second id 4\",    \"valueCommon\": \"common scond id 4\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueSecond\": \"second id 2\",    \"valueCommon\": \"common first id 2\",    \"valueFirst\": \"first id 2\"  },  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueSecond\": \"second id 1\",    \"valueCommon\": \"common first id 1\",    \"valueFirst\": \"first id 1\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }]}";
        var data = JObject.Parse(
            "{\"first\":[{\"key\":{\"id\":\"1\"},\"item\":\"1\",\"valueFirst\":\"first id 1\",\"valueCommon\":\"common first id 1\"},{\"key\":{\"id\":\"2\"},\"item\":2,\"valueFirst\":\"first id 2\",\"valueCommon\":\"common first id 2\"},{\"key\":{\"id\":\"3\"},\"item\":3.1,\"valueFirst\":\"first id 3\",\"valueCommon\":\"common first id 3\"}],\"second\":[{\"key\":{\"id\":\"4\"},\"item\":\"4\",\"valueSecond\":\"second id 4\",\"valueCommon\":\"common scond id 4\"},{\"key\":{\"id\":\"2\"},\"item\":5,\"valueSecond\":\"second id 2\",\"valueCommon\":\"common scond id 2\"},{\"key\":{\"id\":\"1\"},\"item\":3.1,\"valueSecond\":\"second id 1\",\"valueCommon\":\"common scond id 1\"}]}");

        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings
        {
            ArraySettings = new List<MergeArraySettings>
                {new MergeArraySettings {ArrayPath = "$.second"}}
        };
        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
        Assert.IsFalse(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeysAndOnlyStructure()
    {
        var expectedResult =
            "{\"first\": [  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueFirst\": \"first id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueFirst\": \"first id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }],\"second\": [  {    \"key\": {      \"id\": \"4\"    },    \"item\": \"4\",    \"valueSecond\": \"second id 4\",    \"valueCommon\": \"common scond id 4\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 5,    \"valueSecond\": \"second id 2\",    \"valueCommon\": \"common scond id 2\",    \"valueFirst\": \"first id 2\"  },  {    \"key\": {      \"id\": \"1\"    },    \"item\": 3.1,    \"valueSecond\": \"second id 1\",    \"valueCommon\": \"common scond id 1\",    \"valueFirst\": \"first id 1\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }]}";
        var data = JObject.Parse(
            "{\"first\":[{\"key\":{\"id\":\"1\"},\"item\":\"1\",\"valueFirst\":\"first id 1\",\"valueCommon\":\"common first id 1\"},{\"key\":{\"id\":\"2\"},\"item\":2,\"valueFirst\":\"first id 2\",\"valueCommon\":\"common first id 2\"},{\"key\":{\"id\":\"3\"},\"item\":3.1,\"valueFirst\":\"first id 3\",\"valueCommon\":\"common first id 3\"}],\"second\":[{\"key\":{\"id\":\"4\"},\"item\":\"4\",\"valueSecond\":\"second id 4\",\"valueCommon\":\"common scond id 4\"},{\"key\":{\"id\":\"2\"},\"item\":5,\"valueSecond\":\"second id 2\",\"valueCommon\":\"common scond id 2\"},{\"key\":{\"id\":\"1\"},\"item\":3.1,\"valueSecond\":\"second id 1\",\"valueCommon\":\"common scond id 1\"}]}");

        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings
        {
            Strategy = MergeSettings.STRATEGY_ONLY_STRUCTURE,
            ArraySettings = new List<MergeArraySettings>
                {new MergeArraySettings {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id"}}}
        };

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeysAndOnlyValues()
    {
        var expectedResult =
            "{\"first\": [  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueFirst\": \"first id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueFirst\": \"first id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }],\"second\": [  {    \"key\": {      \"id\": \"4\"    },    \"item\": \"4\",    \"valueSecond\": \"second id 4\",    \"valueCommon\": \"common scond id 4\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueSecond\": \"second id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueSecond\": \"second id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }]}";
        var data = JObject.Parse(
            "{\"first\":[{\"key\":{\"id\":\"1\"},\"item\":\"1\",\"valueFirst\":\"first id 1\",\"valueCommon\":\"common first id 1\"},{\"key\":{\"id\":\"2\"},\"item\":2,\"valueFirst\":\"first id 2\",\"valueCommon\":\"common first id 2\"},{\"key\":{\"id\":\"3\"},\"item\":3.1,\"valueFirst\":\"first id 3\",\"valueCommon\":\"common first id 3\"}],\"second\":[{\"key\":{\"id\":\"4\"},\"item\":\"4\",\"valueSecond\":\"second id 4\",\"valueCommon\":\"common scond id 4\"},{\"key\":{\"id\":\"2\"},\"item\":5,\"valueSecond\":\"second id 2\",\"valueCommon\":\"common scond id 2\"},{\"key\":{\"id\":\"1\"},\"item\":3.1,\"valueSecond\":\"second id 1\",\"valueCommon\":\"common scond id 1\"}]}");

        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings
        {
            Strategy = MergeSettings.STRATEGY_ONLY_VALUES,
            ArraySettings = new List<MergeArraySettings>
                {new MergeArraySettings {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id"}}}
        };

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexArraysUniquewithKeysWithDoubleDotNotations()
    {
        var expectedResult =
            "{\"first\": [  {    \"PP\": {      \"PP_EXTERN\": 1,      \"PP_MYBRA\": \"extern 1\"    }  },  {    \"PP\": {      \"PP_EXTERN\": 2,      \"PP_MYBRA\": \"extern 2\"    }  }],\"second\": {  \"PP\": {    \"PP_EXTERN\": 1,    \"PP_MYBRA\": \"extern 1\"  },  \"previous\": {    \"PP\": {      \"PP_EXTERN\": 2,      \"PP_MYBRA\": \"extern 2\"    }  }}}";
        var data = JObject.Parse(
            "{\"first\":[{\"PP\":{\"PP_EXTERN\":1,\"PP_MYBRA\":\"extern 1\"}},{\"PP\":{\"PP_EXTERN\":2,\"PP_MYBRA\":\"extern 2\"}}],\"second\":{\"PP\":{\"PP_EXTERN\":1},\"previous\":{\"PP\":{\"PP_EXTERN\":2}}}}");

        var path = "$.first..PP";
        var targetPath = "$.second..PP";
        var mergeSettings = new MergeSettings
        {
            MatchSettings = new MatchSettings
            {
                KeyPaths = new List<string> {"PP_EXTERN"}
            }
        };

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexPropertiesInObject()
    {
        var expectedResult =
            "{\"first\": {  \"common\": 4,  \"onlyFirst\": \"first value\"},\"second\": {  \"common\": 4,  \"onlySecond\": \"second value\",  \"onlyFirst\": \"first value\"}}";
        var data =
            JObject.Parse(
                "{\"first\":{\"common\":4,\"onlyFirst\":\"first value\"},\"second\":{\"common\":5,\"onlySecond\":\"second value\"}}");


        var path = "$.first";
        var targetPath = "$.second";

        var result = new Merge(path, targetPath).Execute(data, executeOptions);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexPropertiesInObjectOnlyStructure()
    {
        var expectedResult =
            "{\"first\": {  \"common\": 4,  \"onlyFirst\": \"first value\"},\"second\": {  \"common\": 5,  \"onlySecond\": \"second value\",  \"onlyFirst\": \"first value\"}}";
        var data =
            JObject.Parse(
                "{\"first\":{\"common\":4,\"onlyFirst\":\"first value\"},\"second\":{\"common\":5,\"onlySecond\":\"second value\"}}");
        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings {Strategy = MergeSettings.STRATEGY_ONLY_STRUCTURE};

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
    }

    [Test]
    public void MergeTwoComplexPropertiesInObjectOnlyValues()
    {
        var expectedResult =
            "{\"first\": {  \"common\": 4,  \"onlyFirst\": \"first value\"},\"second\": {  \"common\": 4,  \"onlySecond\": \"second value\"}}";
        var data =
            JObject.Parse(
                "{\"first\":{\"common\":4,\"onlyFirst\":\"first value\"},\"second\":{\"common\":5,\"onlySecond\":\"second value\"}}");

        var path = "$.first";
        var targetPath = "$.second";
        var mergeSettings = new MergeSettings {Strategy = MergeSettings.STRATEGY_ONLY_VALUES};

        var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
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