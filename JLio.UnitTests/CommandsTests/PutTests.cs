using System.Linq;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests
{
    public class PutTests
    {
        private JToken data;

        private IExecutionContext executeOptions;

        [SetUp]
        public void Setup()
        {
            executeOptions = ExecutionContext.CreateDefault();
            data = JToken.Parse(
                "{\r\n  \"myString\": \"demo2\",\r\n  \"myNumber\": 2.2,\r\n  \"myInteger\": 20,\r\n  \"myObject\": {\r\n    \"myObject\": {\"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]},\r\n    \"myArray\": [\r\n      2,\r\n      20,\r\n      200,\r\n      2000\r\n    ]\r\n  },\r\n  \"myArray\": [\r\n    2,\r\n    20,\r\n    200,\r\n    2000\r\n  ],\r\n  \"myBoolean\": true,\r\n  \"myNull\": null\r\n}");
        }

        [TestCase("$.myObject.newItem", "newData")]
        [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
        [TestCase("$.myArray", "newData")]
        [TestCase("$.myNull", "newData")]
        [TestCase("$..myObject.newItem", "newData")]
        [TestCase("$..myArray", "newData")]
        [TestCase("$.newProperty", "newData")]
        [TestCase("$..myObject[?(@.myArray)].newProperty)",
            "newData")] // this is not working yet  need to consult newtonsoft
        public void CanPutValues(string path, string value)
        {
            var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Put(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestCase("$.myObject.newItem", "newData")]
        [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
        [TestCase("$.myArray", "newData")]
        [TestCase("$..myObject.newItem", "newData")]
        [TestCase("$..myArray", "newData")]
        [TestCase("$.newProperty", "newData")]
        public void CanPutCorrectValues(string path, string value)
        {
            var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Put(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
            Assert.IsTrue(data.SelectTokens(path).Any());
        }

        [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
        public void CanPutCorrectValuesWithOtherConstructor(string path, string value)
        {
            var result = new Put(path, new JValue(value)).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
            Assert.IsTrue(data.SelectTokens(path).Any());
        }

        [TestCase("$.myObject.newItem", "newData")]
        public void CanPutCorrectValuesWithEmptyConstructor(string path, string value)
        {
            var command = new Put
                {Path = path, Value = new FunctionSupportedValue(new FixedValue(new JValue(value)))};

            var result = command.Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
            Assert.IsTrue(data.SelectTokens(path).Any());
        }

        [TestCase("$.newProperty", "{\"demo\" : 3}")]
        public void CanPutCorrectValuesAsTokens(string path, string value)
        {
            var tokenToAdd = JToken.Parse(value);
            var valueToAdd = new FunctionSupportedValue(new FixedValue(tokenToAdd));
            var result = new Put(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
            Assert.IsTrue(data.SelectTokens(path).Any());
            Assert.IsTrue(JToken.DeepEquals(data.SelectToken(path), tokenToAdd));
        }

        [TestCase("", "newData", "Path property for put command is missing")]
        [TestCase("", null, "Path property for put command is missing")]
        public void CanExecuteWithArgumentsNotProvided(string path, string value, string message)
        {
            var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Put(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(executeOptions.GetLogEntries().Any(l => l.Message == message));
        }

        [Test]
        public void CanUseFluentApi()
        {
            var script = new JLioScript()
                    .Put(new JValue("new Value"))
                    .OnPath("$.demo")
                    .Put(new Datetime())
                    .OnPath("$.this.is.a.long.path.with.a.date")
                ;
            var result = script.Execute(new JObject());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.demo")?.Type, JTokenType.Null);
            Assert.AreNotEqual(result.Data.SelectToken("$.this.is.a.long.path.with.a.date")?.Type, JTokenType.Null);
        }
    }
}