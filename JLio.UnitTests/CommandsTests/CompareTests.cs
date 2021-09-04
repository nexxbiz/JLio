using JLio.Commands.Advanced;
using JLio.Commands.Advanced.Models;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests
{
    public class CompareTests
    {
        private JLioExecutionOptions executeOptions;

        [SetUp]
        public void Setup()
        {
            executeOptions = JLioExecutionOptions.CreateDefault();
        }

        [TestCase("{\"first\":true,\"second\":true}", false)]
        [TestCase("{\"first\":true,\"second\":false}", true)]
        [TestCase("{\"first\":false,\"second\":true}", true)]
        [TestCase("{\"first\":false,\"second\":false}", false)]
        [TestCase("{\"first\":1,\"second\":1}", false)]
        [TestCase("{\"first\":1,\"second\":2}", true)]
        [TestCase("{\"first\":2,\"second\":1}", true)]
        [TestCase("{\"first\":2,\"second\":2}", false)]
        [TestCase("{\"first\":1.1,\"second\":1.1}", false)]
        [TestCase("{\"first\":1.2,\"second\":2.1}", true)]
        [TestCase("{\"first\":2.1,\"second\":1.1}", true)]
        [TestCase("{\"first\":2.1,\"second\":2.1}", false)]
        [TestCase("{\"first\": [1,2],\"second\": [1,2]}", false)]
        [TestCase("{\"first\": [1,2,3],\"second\": [1,2]}", true)]
        [TestCase("{\"first\": {\"a\" : 1 },\"second\": {\"a\" : 1 }}", false)]
        [TestCase("{\"first\": {\"a\" : 1, \"b\" : 1 },\"second\": {\"a\" : 1 }}", true)]
        [TestCase("{\"first\": {\"a\" : 1 },\"second\": {\"a\" : 1, \"b\" : 1 }}", true)]
        [TestCase("{\"first\": {\"a\" : 1 ,\"b\" : 1 },\"second\": {\"a\" : 1, \"b\" : 1 }}", false)]
        [TestCase("{\"first\":[1,2,2,3],\"second\":[2,3,3,4]}", true)]
        public void CanCompare(string dataText, bool different)
        {
            var data = JToken.Parse(dataText);
            var result = new Compare
            {
                FirstPath = "$.first", SecondPath = "$.second", ResultPath = "$.result",
                Settings = new Compare.CompareSettings()
            }.Execute(data, executeOptions);

            var compareResults = result.Data.SelectToken("$.result")?.ToObject<CompareResults>();

            Assert.IsNotNull(result);
            Assert.IsNotNull(compareResults);
            Assert.AreEqual(different, compareResults?.ContainsIsDifferenceResult());
        }

        [TestCase("{\"first\":{\"sub\":{\"a\":1}},\"second\":{\"a\":1}}", false)]
        public void CanCompareOnDifferentLevels(string dataText, bool different)
        {
            var data = JToken.Parse(dataText);
            var result = new Compare
            {
                FirstPath = "$.first.sub",
                SecondPath = "$.second",
                ResultPath = "$.result",
                Settings = new Compare.CompareSettings()
            }.Execute(data, executeOptions);

            var compareResults = result.Data.SelectToken("$.result")?.ToObject<CompareResults>();

            Assert.IsNotNull(result);
            Assert.IsNotNull(compareResults);
            Assert.AreEqual(different, compareResults?.ContainsIsDifferenceResult());
        }
    }
}