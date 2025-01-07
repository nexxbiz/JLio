using System.IO;
using System.Linq;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;


namespace JLio.UnitTests.CommandsTestV2.PutTests
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

        [Test]
        public void canPutObjectValueOnNewproperty()
        {
            ExecuteTest(nameof(canPutObjectValueOnNewproperty));
        }

        [Test]
        public void canPutStringValueOnNewsubitem()
        {
            ExecuteTest(nameof(canPutStringValueOnNewsubitem));
        }

        [Test]
        public void canPutStringValueOnNewproperty()
        {
            ExecuteTest(nameof(canPutStringValueOnNewproperty));
        }

        [Test]
        public void canPutStringValueOnMynull()
        {
            ExecuteTest(nameof(canPutStringValueOnMynull));
        }

        [Test]
        public void canPutStringValueOnNewitem()
        {
            ExecuteTest(nameof(canPutStringValueOnNewitem));
        }

        [Test]
        public void canPutStringValueOnMyarray()
        {
            ExecuteTest(nameof(canPutStringValueOnMyarray));
        }

        [Test]
        public void canPutStringValueOnNewitem_1()
        {
            ExecuteTest(nameof(canPutStringValueOnNewitem_1));
        }

        [Test]
        public void canPutStringValueOnMyarray_1()
        {
            ExecuteTest(nameof(canPutStringValueOnMyarray_1));
        }

        [Test]
        public void canPutStringValueOnNewitem_2()
        {
            ExecuteTest(nameof(canPutStringValueOnNewitem_2));
        }

        [Test]
        public void canPutStringValueOnNewsubitem_1()
        {
            ExecuteTest(nameof(canPutStringValueOnNewsubitem_1));
        }

        [Test]
        public void canPutStringValueOnNewitem_3()
        {
            ExecuteTest(nameof(canPutStringValueOnNewitem_3));
        }

        [Test]
        public void canPutStringValueOnNewsubitem_2()
        {
            ExecuteTest(nameof(canPutStringValueOnNewsubitem_2));
        }

        [Test]
        public void canPutStringValueOnMyarray_2()
        {
            ExecuteTest(nameof(canPutStringValueOnMyarray_2));
        }

        [Test]
        public void canPutStringValueOnNewitem_4()
        {
            ExecuteTest(nameof(canPutStringValueOnNewitem_4));
        }

        [Test]
        public void canPutStringValueOnMyarray_3()
        {
            ExecuteTest(nameof(canPutStringValueOnMyarray_3));
        }

        [Test]
        public void canPutStringValueOnNewproperty_1()
        {
            ExecuteTest(nameof(canPutStringValueOnNewproperty_1));
        }

        [Test]
        public void canPutStringValueOnNewproperty_2()
        {
            ExecuteTest(nameof(canPutStringValueOnNewproperty_2));
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
            { Path = path, Value = new FunctionSupportedValue(new FixedValue(new JValue(value))) };

            var result = command.Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(data.SelectTokens(path).All(i => i.Type != JTokenType.Null));
            Assert.IsTrue(data.SelectTokens(path).Any());
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


        private void ExecuteTest(string testName)
        {
            var testCase = TestCaseLoader.LoadTestCase<PutTestCase>(GetTestCaseFilePath(testName));
            Assert.IsNotNull(testCase, $"Test case '{testCase.Name}' data should not be null.");

            var data = testCase.Data;
            var executeOptions = ExecutionContext.CreateDefault();
            var valueToAdd = new FunctionSupportedValue(new FixedValue(testCase.Value));
            var putCommand = new Put(testCase.Path, valueToAdd);
            var result = putCommand.Execute(data, executeOptions);

            Assert.AreEqual(testCase.ExpectedSuccess, result.Success, $"Test case '{testCase.Name}' failed: Success mismatch.");

            if (testCase.ExpectedData != null)
            {
                Assert.IsTrue(JToken.DeepEquals(data, testCase.ExpectedData), $"Test case '{testCase.Name}' failed: Data mismatch.");
            }
        }

        private string GetTestCaseFilePath(string testName)
        {
            var directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "CommandsTestV2", nameof(PutTests));
            var filePath = Path.Combine(directory, $"{testName}.json");
            if (!File.Exists(filePath))
                Assert.Fail($"Test case file '{filePath}' does not exist.");
            return filePath;
        }
    }
}
