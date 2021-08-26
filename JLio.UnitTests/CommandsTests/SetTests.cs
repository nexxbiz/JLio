using System.Linq;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests
{
    public class SetTests
    {
        private JToken data;
        private JLioExecutionOptions executeOptions;

        [SetUp]
        public void Setup()
        {
            executeOptions = JLioExecutionOptions.CreateDefault();
            data = JToken.Parse(
                "{\r\n  \"myString\": \"demo2\",\r\n  \"myNumber\": 2.2,\r\n  \"myInteger\": 20,\r\n  \"myObject\": {\r\n    \"myObject\": {\"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]},\r\n    \"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]\r\n  },\r\n  \"myArray\": [\r\n    2,\r\n    20,\r\n    200,\r\n    2000\r\n  ],\r\n  \"myBoolean\": true,\r\n  \"myNull\": null\r\n}");
        }

        [TestCase("$.myObject.myArray", "newData")]
        [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
        [TestCase("$.myArray", "newData")]
        [TestCase("$.myNull", "newData")]
        [TestCase("$..myArray", "newData")]
        public void CanSetValues(string path, string value)
        {
            var valueToSet = new JLioFunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Set(path, valueToSet).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestCase("$.myObject", "newData")]
        [TestCase("$.myArray", "newData")]
        [TestCase("$..myObject", "newData")]
        [TestCase("$..myArray", "newData")]
        public void CanSetCorrectValues(string path, string value)
        {
            var valueToSet = new JLioFunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Set(path, valueToSet).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
            Assert.IsTrue(data.SelectTokens(path).Any());
        }

        [TestCase("", "newData", "Path property for set command is missing")]
        [TestCase("", null, "Path property for set command is missing")]
        public void CanExecuteWithArgumentsNotProvided(string path, string value, string message)
        {
            var valueToAdd = new JLioFunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Set(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.Any(l => l.Message == message));
        }

        [Test]
        public void CanUseFluentApi()
        {
            var data = JObject.Parse("{ \"demo\" : \"old value\" , \"demo2\" : \"old value\" }");
            var script = new JLioScript()
                    .Set(new JValue("new Value"))
                    .OnPath("$.demo")
                    .Set(new DatetimeFunction())
                    .OnPath("$.demo2")
                ;
            var result = script.Execute(data);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.demo").Type, JTokenType.Null);
            Assert.AreEqual(result.Data.SelectToken("$.demo").Value<string>(), "new Value");
            Assert.AreNotEqual(result.Data.SelectToken("$.demo2").Type, JTokenType.Null);
        }
    }
}