using System.Linq;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests
{
    public class RemoveTests
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

        [TestCase("$.myString")]
        [TestCase("$.myNumber")]
        [TestCase("$.myInteger")]
        [TestCase("$.myBoolean")]
        [TestCase("$.myArray")]
        [TestCase("$.myObject.myArray")]
        [TestCase("$.myNull")]
        [TestCase("$..myObject.myArray")]
        [TestCase("$..myArray")]
        public void CanRemoveProperty(string path)
        {
            var result = new Remove(path).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, data.SelectTokens(path).Count());
        }

        [TestCase("$.myArray[100]")]
        [TestCase("$.myArray[0]")]
        public void CanRemoveEdgeCasesProperty(string path)
        {
            var result = new Remove(path).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        [TestCase("$.myArray[1]", "$.myArray", "[2,200,2000]")]
        [TestCase("$.myArray[?(@ == 20)]", "$.myArray", "[2,200,2000]")]
        [TestCase("$.myArray[?(@ > 20)]", "$.myArray", "[2,20]")]
        public void CanRemoveValueFromArray(string path, string checkpath, string expectedValue)
        {
            var result = new Remove(path).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValue), data.SelectToken(checkpath)));
        }

        [TestCase("", "Path property for remove command is missing")]
        public void CanExecuteWithArgumentsNotProvided(string path, string message)
        {
            var result = new Remove(path).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.Any(l => l.Message == message));
        }

        [Test]
        public void CanUseFluentApi()
        {
            var data = JObject.Parse("{ \"demo\" : \"old value\" , \"demo2\" : \"old value\" }");
            var script = new JLioScript()
                .Remove("$.demo")
                .Remove("$.demo2");

            var result = script.Execute(data);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Data.SelectToken("$.demo"));
            Assert.IsNull(result.Data.SelectToken("$.demo2"));
        }
    }
}