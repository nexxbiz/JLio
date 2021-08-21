using JLio.Client;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JLio.UnitTests.CommandsTests
{
    public class AddTests
    {

        private JLioExecutionOptions executeOptions;
        private JToken data;

        [SetUp]
        public void Setup()
        {
            executeOptions = JLioExecutionOptions.CreateDefault();
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
        public void CanAddValues(string path, string value)
        {
            var valueToAdd = new JLioFunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Add(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestCase("$.myObject.newItem", "newData")]
        [TestCase("$.NewObject.newItem.NewSubItem", "newData")]
        [TestCase("$.myArray", "newData")]
        [TestCase("$..myObject.newItem", "newData")]
        [TestCase("$..myArray", "newData")]
        [TestCase("$.newProperty", "newData")]
        public void CanAddCorrectValues(string path, string value)
        {
            var valueToAdd = new JLioFunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Add(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
            Assert.IsTrue(data.SelectTokens(path).Any());
        }

        [TestCase("", "newData", "Path property for add command is missing")]
        [TestCase("", null, "Path property for add command is missing")]
        public void CanExecuteWithArgumentsNotProvided(string path, string value, string message)
        {
            var valueToAdd = new JLioFunctionSupportedValue(new FixedValue(new JValue(value)));
            var result = new Add(path, valueToAdd).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.Any(l => l.Message == message));
        }

        [Test]
        public void CanUseFluentApi()
        {
            var script = new JLioScript()
                .AddScriptLine()
                 .Add(new JValue("new Value"))
                 .OnPath("$.demo")
                .AddScriptLine()
                 .Add(new DatetimeFunction())
                 .OnPath("$.this.is.a.long.path.with.a.date")
               ;
            var result = script.Execute(new JObject());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.demo").Type, JTokenType.Null);
            Assert.AreNotEqual(result.Data.SelectToken("$.this.is.a.long.path.with.a.date").Type, JTokenType.Null);

        }
    }
}

