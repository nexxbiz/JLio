using System.Collections.Generic;
using System.IO;
using JLio.Commands;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTestV2
{
    public class PutTests
    {

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


        private void ExecuteTest(string testName)
        {
            var testCase = TestCaseLoader.LoadTestCase(GetTestCaseFilePath(testName));
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
