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
        public void canPutToRoot()
        {
            ExecuteTest(nameof(canPutToRoot));
        }

        [Test]
        public void canPutNestedProperty()
        {
            ExecuteTest(nameof(canPutNestedProperty));
        }

        [Test]
        public void canPutOnArray()
        {
            ExecuteTest(nameof(canPutOnArray));
        }

        [Test]
        public void canPutOverwriteNull()
        {
            ExecuteTest(nameof(canPutOverwriteNull));
        }

        [Test]
        public void canPutRecursiveProperty()
        {
            ExecuteTest(nameof(canPutRecursiveProperty));
        }

        [Test]
        public void canPutRecursiveArray()
        {
            ExecuteTest(nameof(canPutRecursiveArray));
        }

        [Test]
        public void canPutTopLevelProperty()
        {
            ExecuteTest(nameof(canPutTopLevelProperty));
        }

//todo: Bug
        //[Test]
        //public void canPutToNestedArray()
        //{
        //    ExecuteTest(nameof(canPutToNestedArray));
        //}

        [Test]
        public void canPutArrayToRoot()
        {
            ExecuteTest(nameof(canPutArrayToRoot));
        }

        [Test]
        public void canPutDeeplyNestedValue()
        {
            ExecuteTest(nameof(canPutDeeplyNestedValue));
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
            Assert.IsNotNull(testCase, $"Test case '{testCase?.Name}' data should not be null.");

            var data = testCase.Data;
            var executeOptions = ExecutionContext.CreateDefault();
            var valueToAdd = new FunctionSupportedValue(new FixedValue(testCase.Value));
            var putCommand = new Put(testCase.Path, valueToAdd);
            var result = putCommand.Execute(data, executeOptions);

            Assert.AreEqual(testCase.ExpectedSuccess, result.Success,
                $"Test case '{testCase.Name}' failed: Success mismatch. Input data: {testCase.Data}");

            if (testCase.ExpectedData != null)
            {
                if (!JToken.DeepEquals(data, testCase.ExpectedData))
                {
                    Assert.Fail($"Test case '{testCase.Name}' failed: Data mismatch. \nExpected: {testCase.ExpectedData} \nActual: {data}");
                }
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
