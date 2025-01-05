using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTestV2
{
    public static partial class TestCaseLoader
    {
        public static TestCase LoadTestCase(string filePath)
        {
            try
            {
                var testCaseJson = File.ReadAllText(filePath);
                var testCase = JsonConvert.DeserializeObject<TestCase>(testCaseJson, new JsonSerializerSettings
                {
                    Converters = { new JTokenConverter() }
                });
                testCase.Name = Path.GetFileNameWithoutExtension(filePath);
                return testCase;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to load or deserialize test case from file: {filePath}\n{ex.Message}");
                return null; // This line is unreachable due to Assert.Fail.
            }
        }
    }
}
