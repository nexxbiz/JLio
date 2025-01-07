using System.IO;
using JLio.Commands;
using JLio.Core;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTestV2.PutTests
{
    public class AddTests
    {
        [Test]
        public void canAddObjectValueOnNewproperty()
        {
            ExecuteTest(nameof(canAddObjectValueOnNewproperty));
        }

        [Test]
        public void canAddValueToExistingArray()
        {
            ExecuteTest(nameof(canAddValueToExistingArray));
        }




        [Test]
        public void canAddStringValueToObject()
        {
            ExecuteTest(nameof(canAddStringValueToObject));
        }

        [Test]
        public void canAddNestedObject()
        {
            ExecuteTest(nameof(canAddNestedObject));
        }

        [Test]
        public void canAddNullValue()
        {
            ExecuteTest(nameof(canAddNullValue));
        }

        [Test]
        public void canAddValueToNonExistingArray()
        {
            ExecuteTest(nameof(canAddValueToNonExistingArray));
        }

        [Test]
        public void canAddIntegerValueToObject()
        {
            ExecuteTest(nameof(canAddIntegerValueToObject));
        }

        [Test]
        public void canAddDateValueToObject()
        {
            ExecuteTest(nameof(canAddDateValueToObject));
        }

        [Test]
        public void canAddValueUsingComplexPath()
        {
            ExecuteTest(nameof(canAddValueUsingComplexPath));
        }

        [Test]
        public void canAddValueWithRecursivePath()
        {
            ExecuteTest(nameof(canAddValueWithRecursivePath));
        }


        [Test]
        public void canAddToRoot()
        {
            ExecuteTest(nameof(canAddToRoot));
        }
//#TODO:bug
        //[Test]
        //public void canAddToNestedArray()
        //{
        //    ExecuteTest(nameof(canAddToNestedArray));
        //}

        [Test]
        public void canAddArrayToRoot()
        {
            ExecuteTest(nameof(canAddArrayToRoot));
        }

        [Test]
        public void canAddToNonExistingArray()
        {
            ExecuteTest(nameof(canAddToNonExistingArray));
        }

        [Test]
        public void canAddDeeplyNestedValue()
        {
            ExecuteTest(nameof(canAddDeeplyNestedValue));
        }




        private void ExecuteTest(string testName)
        {
            var testCase = TestCaseLoader.LoadTestCase<AddTestCase>(GetTestCaseFilePath(testName));
            Assert.IsNotNull(testCase, $"Test case '{testCase?.Name}' data should not be null.");

            var data = testCase.Data;
            var executeOptions = ExecutionContext.CreateDefault();
            var valueToAdd = new FunctionSupportedValue(new FixedValue(testCase.Value));
            var putCommand = new Add(testCase.Path, valueToAdd);
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
            var directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "CommandsTestV2", nameof(AddTests));
            var filePath = Path.Combine(directory, $"{testName}.json");
            if (!File.Exists(filePath))
            {
                Assert.Fail($"Test case file '{filePath}' does not exist. Ensure the test file is in the correct directory.");
            }
            return filePath;
        }
    }
}
