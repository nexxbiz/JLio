using System.Linq;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests;

public class CopyMoveTests
{
    private JToken data;
    private IExecutionContext executeOptions;

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        data = JToken.Parse(
            "{ \"myString\": \"demo2\", \"myNumber\": 2.2, \"myInteger\": 20, \"myObject\": { \"myObject\": {\"myArray\": [ 2, 20, 200, 2000 ]}, \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ], \"myBoolean\": true, \"myNull\": null}");
    }

    [TestCase("$.myString", "$.myNewObject.newItem", "'demo2'")]
    [TestCase("$.myNumber", "$.myNewObject.newItem", "2.2")]
    [TestCase("$.myInteger", "$.myNewObject.newItem", "20")]
    [TestCase("$.myObject", "$.myNewObject.newItem",
        "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
    [TestCase("$.myArray", "$.myNewObject.newItem", "[ 2, 20, 200, 2000 ]")]
    [TestCase("$.myBoolean", "$.myNewObject.newItem", "true")]
    [TestCase("$.myNull", "$.myNewObject.newItem", "null")]
    [TestCase("$.myString", "$.myObject", "'demo2'")]
    [TestCase("$.myNumber", "$.myObject", "2.2")]
    [TestCase("$.myInteger", "$.myObject", "20")]
    [TestCase("$.myObject", "$.myObject",
        "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
    [TestCase("$.myArray", "$.myObject", "[ 2, 20, 200, 2000 ]")]
    [TestCase("$.myBoolean", "$.myObject", "true")]
    [TestCase("$.myNull", "$.myObject", "null")]
    public void PropertyCopyTests(string from, string to, string expectedValueToPath)
    {
        var result = new Copy(from, to).Execute(data, executeOptions);


        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
        Assert.IsTrue(JToken.DeepEquals(data.SelectToken(from), data.SelectToken(to)));
    }

    [TestCase("$.myString", "$.myArray", "[ 2, 20, 200, 2000, 'demo2' ]")]
    [TestCase("$.myNumber", "$.myArray", "[ 2, 20, 200, 2000, 2.2]")]
    [TestCase("$.myInteger", "$.myArray", "[ 2, 20, 200, 2000, 20]")]
    [TestCase("$.myObject", "$.myArray",
        "[ 2, 20, 200, 2000, { \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }]")]
    [TestCase("$.myArray[*]", "$.myArray", "[ 2, 20, 200, 2000 ,2, 20, 200, 2000 ]")]
    [TestCase("$.myArray[?(@ > 20)]", "$.myArray", "[ 2, 20, 200, 2000 , 200, 2000 ]")]
    [TestCase("$.myArray", "$.myArray", "[ 2, 20, 200, 2000 ,[2, 20, 200, 2000] ]")]
    [TestCase("$.myBoolean", "$.myArray", "[ 2, 20, 200, 2000, true]")]
    [TestCase("$.myNull", "$.myArray", "[ 2, 20, 200, 2000, null]")]
    public void PropertyCopyArrayTests(string from, string to, string expectedValueToPath)
    {
        var result = new Copy(from, to).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
    }

    [TestCase("$.myString", "$.myArray", "[ 2, 20, 200, 2000, 'demo2' ]")]
    [TestCase("$.myNumber", "$.myArray", "[ 2, 20, 200, 2000, 2.2]")]
    [TestCase("$.myInteger", "$.myArray", "[ 2, 20, 200, 2000, 20]")]
    [TestCase("$.myObject", "$.myArray",
        "[ 2, 20, 200, 2000, { \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }]")]
    [TestCase("$.myArray[*]", "$.myArray", "[ 2, 20, 200, 2000 ]")]
    [TestCase("$.myObject.myArray[?(@ > 20)]", "$.myArray", "[ 2, 20, 200, 2000 , 200, 2000 ]")]
    [TestCase("$.myBoolean", "$.myArray", "[ 2, 20, 200, 2000, true]")]
    [TestCase("$.myNull", "$.myArray", "[ 2, 20, 200, 2000, null]")]
    public void PropertyMoveArrayTests(string from, string to, string expectedValueToPath)
    {
        var result = new Move(from, to).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
    }

    [TestCase("$.myObject", "$",
        "{ \"myObject\": {\"myArray\": [ 2, 20, 200, 2000 ]}, \"myArray\": [ 2, 20, 200, 2000 ] }")]
    public void PropertyMoveToRootTests(string from, string to, string expectedValueToPath)
    {
        var result = new Move(from, to).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
    }

    [TestCase("$.myString", "$.mystring", "'demo2'")]
    [TestCase("$.myString", "$.myNewObject.newItem", "'demo2'")]
    [TestCase("$.myNumber", "$.myNewObject.newItem", "2.2")]
    [TestCase("$.myInteger", "$.myNewObject.newItem", "20")]
    [TestCase("$.myObject", "$.myNewObject.newItem",
        "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
    [TestCase("$.myArray", "$.myNewObject.newItem", "[ 2, 20, 200, 2000 ]")]
    [TestCase("$.myBoolean", "$.myNewObject.newItem", "true")]
    [TestCase("$.myNull", "$.myNewObject.newItem", "null")]
    [TestCase("$.myString", "$.myObject", "'demo2'")]
    [TestCase("$.myNumber", "$.myObject", "2.2")]
    [TestCase("$.myInteger", "$.myObject", "20")]
    [TestCase("$.myObject", "$.myObject",
        "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
    [TestCase("$.myArray", "$.myObject", "[ 2, 20, 200, 2000 ]")]
    [TestCase("$.myBoolean", "$.myObject", "true")]
    [TestCase("$.myNull", "$.myObject", "null")]
    public void PropertyMoveTests(string from, string to, string expectedValueToPath)
    {
        var result = new Move(from, to).Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
        Assert.IsTrue(from == to || data.SelectToken(from) == null);
    }

    [TestCase("{\"myData\":[{\"demo\":[{\"demo2\":3}]},{\"demo\":[{\"demo2\":4}]},{\"demo\":[{\"demo2\":5}]}]}",
              "$.myData[*].demo[*].demo2",
              "$.myData[*].demo[*].new",
              "{\"myData\":[{\"demo\":[{\"new\":3}]},{\"demo\":[{\"new\":4}]},{\"demo\":[{\"new\":5}]}]}")]
    [TestCase("{\"myData\":[{\"demo\":[{\"demo2\":{\"oldProperty\":1}}]},{\"demo\":[{\"demo2\":{\"oldProperty\":2}}]},{\"demo\":[{\"demo2\":{\"oldProperty\":3}}]}]}",
              "$.myData[*].demo[*].demo2.oldProperty",
              "$.myData[*].demo[*].new",
              "{\"myData\":[{\"demo\":[{\"demo2\":{},\"new\":1}]},{\"demo\":[{\"demo2\":{},\"new\":2}]},{\"demo\":[{\"demo2\":{},\"new\":3}]}]}")]
    [TestCase("{\"myData\":[{\"demo\":[{\"demo2\":{\"oldProperty\":1},\"newProperty\":[{}]}]},{\"demo\":[{\"demo2\":{\"oldProperty\":2},\"newProperty\":[{}]}]},{\"demo\":[{\"demo2\":{\"oldProperty\":3},\"newProperty\":[{}]}]}]}",
              "$.myData[*].demo[*].demo2.oldProperty",
              "$.myData[*].demo[*].newProperty[*].test",
              "{\"myData\":[{\"demo\":[{\"demo2\":{},\"newProperty\":[{\"test\":1}]}]},{\"demo\":[{\"demo2\":{},\"newProperty\":[{\"test\":2}]}]},{\"demo\":[{\"demo2\":{},\"newProperty\":[{\"test\":3}]}]}]}")]
    public void CanCopyMovePropertiesInAnLayeredArray(string startobject, string moveFrom, string moveTo, string expectedValue)
    {
        var startObject = startobject;
        var result =
            new Move(moveFrom,moveTo).Execute(
                JToken.Parse(startObject), executeOptions);
        Assert.IsTrue(JToken.DeepEquals(result.Data,
            JToken.Parse(
                expectedValue)));
    }

    [TestCase(
       "{\"firstArray\":[{\"target\":[],\"secondArray\":[{\"id\":\"item1\",\"sub\":{\"name\":\"item 1\"}},{\"id\":\"item2\",\"sub\":{\"name\":\"item 2\"}}]},{\"target\":[],\"secondArray\":[{\"id\":\"item3\",\"sub\":{\"name\":\"item 3\"}},{\"id\":\"item4\",\"sub\":{\"name\":\"item 4\"}}]}]}",
       "$.firstArray[*].secondArray[*].sub",
       "$.firstArray[*].target",
       "{\"firstArray\":[{\"target\":[{\"name\":\"item 1\"},{\"name\":\"item 2\"}],\"secondArray\":[{\"id\":\"item1\",\"sub\":{\"name\":\"item 1\"}},{\"id\":\"item2\",\"sub\":{\"name\":\"item 2\"}}]},{\"target\":[{\"name\":\"item 3\"},{\"name\":\"item 4\"}],\"secondArray\":[{\"id\":\"item3\",\"sub\":{\"name\":\"item 3\"}},{\"id\":\"item4\",\"sub\":{\"name\":\"item 4\"}}]}]}"
       )]
    public void CanCopyPropertiesInAnLayeredArray(string startobject, string copyFrom, string copyTo, string expectedValue)
    {
        var startObject = startobject;
        var result =
            new Copy(copyFrom, copyTo).Execute(
                JToken.Parse(startObject), executeOptions);
        Assert.IsTrue(JToken.DeepEquals(result.Data,
            JToken.Parse(
                expectedValue)));
    }

    [Test]
    public void CanCopyMovePropertiesInAnArray()
    {
        var startObject =
            "{\"myData\":[{\"demo\":[{\"old\":3}]},{\"demo\":[{\"old\":4}]},{\"demo\":[{\"old\":5}]}]}";
        var result =
            new Move("$.myData[*].demo", "$.myData[*].new").Execute(
                JToken.Parse(startObject), executeOptions);
        Assert.IsTrue(JToken.DeepEquals(result.Data,
            JToken.Parse(
                "{\"myData\":[{\"new\":[{\"old\":3}]},{\"new\":[{\"old\":4}]},{\"new\":[{\"old\":5}]}]}")));
    }

    [Test]
    public void CanCopyMovePropertiesInAnArrayCaseSensitive()
    {
        var startObject =
            "{\"myData\":[{\"demo\":[{\"old\":3}]},{\"demo\":[{\"old\":4}]},{\"demo\":[{\"old\":5}]}]}";
        var result =
            new Move("$.myData[*].demo", "$.myData[*].Demo").Execute(
                JToken.Parse(startObject), executeOptions);
        Assert.IsTrue(JToken.DeepEquals(result.Data,
            JToken.Parse(
                "{\"myData\":[{\"Demo\":[{\"old\":3}]},{\"Demo\":[{\"old\":4}]},{\"Demo\":[{\"old\":5}]}]}")));
    }

    [Test]
    public void CanUseFluentApi()
    {
        var data = JObject.Parse("{ \"demo\" : \"item\" }");

        var script = new JLioScript()
                .Copy("$.demo")
                .To("$.copiedDemo")
                .Move("$.copiedDemo")
                .To("$.result")
            ;
        var result = script.Execute(data);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(JToken.DeepEquals(result.Data.SelectToken("$.demo"), result.Data.SelectToken("$.result")));
        Assert.IsNull(result.Data.SelectToken("$.copiedDemo"));
    }

    [Test]
    public void CanExecuteCopyWithoutParametersSet()
    {
        var command = new Copy();
        var result = command.Execute(JToken.Parse("{\"first\":true,\"second\":true}"), executeOptions);
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeOptions.GetLogEntries().Any());
    }

    [Test]
    public void CanExecuteMoveWithoutParametersSet()
    {
        var command = new Move();
        var result = command.Execute(JToken.Parse("{\"first\":true,\"second\":true}"), executeOptions);
        Assert.IsNotNull(result);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(executeOptions.GetLogEntries().Any());
    }
}