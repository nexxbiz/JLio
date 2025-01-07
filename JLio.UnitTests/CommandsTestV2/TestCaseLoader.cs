using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTestV2
{
    public static partial class TestCaseLoader
    {
        public static T LoadTestCase<T>(string filePath) where T : TestCase
        {
            try
            {
                var testCaseJson = File.ReadAllText(filePath);
                var testCase = JsonConvert.DeserializeObject<T>(testCaseJson, new JsonSerializerSettings
                {
                    Converters = { new JTokenConverter() }
                });
                testCase.Name = Path.GetFileNameWithoutExtension(filePath);
                return testCase;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to load or deserialize test case from file: {filePath}\n{ex.Message}");
            }
            return null;
        }
    }
}
