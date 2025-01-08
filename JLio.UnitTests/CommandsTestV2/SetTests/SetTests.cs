using System.IO;
using JLio.Commands;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;


namespace JLio.UnitTests.CommandsTestV2.PutTests
{
    public class SetTests
    {
        [Test]
        public void canSetArrayToRoot()
        {
            ExecuteTest(nameof(canSetArrayToRoot));
        }
        [Test]
        public void canSetDeeplyNestedValue()
        {
            ExecuteTest(nameof(canSetDeeplyNestedValue));
        }

        [Test]
        public void canSetNestedProperty()
        {
            ExecuteTest(nameof(canSetNestedProperty));
        }
        [Test]
        public void canSetOnArray()
        {
            ExecuteTest(nameof(canSetOnArray));
        }
        [Test]
        public void canSetOverwriteNull()
        {
            ExecuteTest(nameof(canSetOverwriteNull));
        }
        [Test]
        public void canSetRecursiveArray()
        {
            ExecuteTest(nameof(canSetRecursiveArray));
        }
        [Test]
        public void canSetRecursiveProperty()
        {
            ExecuteTest(nameof(canSetRecursiveProperty));
        }
        [Test]
        public void canSetToNestedArray()
        {
            ExecuteTest(nameof(canSetToNestedArray));
        }

      

        private void ExecuteTest(string testName)
        {
            var testCase = TestCaseLoader.LoadTestCase<SetTestCase>(GetTestCaseFilePath(testName));
            Assert.IsNotNull(testCase, $"Test case '{testCase?.Name}' data should not be null.");

            var data = testCase.Data;
            var executeOptions = ExecutionContext.CreateDefault();
            var valueToAdd = new FunctionSupportedValue(new FixedValue(testCase.Value));
            var putCommand = new Set(testCase.Path, valueToAdd);
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
            var directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "CommandsTestV2", nameof(SetTests));
            var filePath = Path.Combine(directory, $"{testName}.json");
            if (!File.Exists(filePath))
                Assert.Fail($"Test case file '{filePath}' does not exist.");
            return filePath;
        }
    }
}
