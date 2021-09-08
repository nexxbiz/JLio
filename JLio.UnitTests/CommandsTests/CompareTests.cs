using System.Collections.Generic;
using System.Linq;
using JLio.Commands.Advanced;
using JLio.Commands.Advanced.Builders;
using JLio.Commands.Advanced.Models;
using JLio.Commands.Advanced.Settings;
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

        [TestCase("{\"first\":1,\"second\":true}", true)]
        [TestCase("{\"first\":\"1\",\"second\":true}", true)]
        [TestCase("{\"first\":10,\"second\":true}", true)]
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
        [TestCase("{\"first\":[[1,2],[4,5]],\"second\":[[5,4],[2,1]]}", false)]
        [TestCase("{\"first\":[[1,2],[4,6]],\"second\":[[5,4],[2,1]]}", true)]
        public void CanComparePrimitives(string dataText, bool different)
        {
            var data = JToken.Parse(dataText);
            var result = new Compare
            {
                FirstPath = "$.first", SecondPath = "$.second", ResultPath = "$.result",
                Settings = new CompareSettings()
            }.Execute(data, executeOptions);

            var compareResults = result.Data.SelectToken("$.result")?.ToObject<CompareResults>();

            Assert.IsNotNull(result);
            Assert.IsNotNull(compareResults);
            Assert.AreEqual(different, compareResults?.ContainsIsDifferenceResult());
        }

        [TestCase(
            "{\"first\":[{\"id\":1,\"value\":1},{\"id\":2,\"value\":2},{\"id\":3,\"value\":3}],\"second\":[{\"id\":3,\"value\":3},{\"id\":2,\"value\":2},{\"id\":1,\"value\":1}]}",
            false)]
        [TestCase(
            "{\"first\":[{\"id\":1,\"value\":2},{\"id\":2,\"value\":2},{\"id\":3,\"value\":3}],\"second\":[{\"id\":3,\"value\":3},{\"id\":2,\"value\":2},{\"id\":1,\"value\":1}]}",
            true)]
        public void CanCompareObjects(string dataText, bool different)
        {
            var data = JToken.Parse(dataText);
            var result = new Compare
            {
                FirstPath = "$.first",
                SecondPath = "$.second",
                ResultPath = "$.result",
                Settings = new CompareSettings
                {
                    ArraySettings = new List<CompareArraySettings>
                    {
                        new CompareArraySettings {ArrayPath = "first", KeyPaths = new List<string> {"id"}}
                    }
                }
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
                Settings = new CompareSettings()
            }.Execute(data, executeOptions);

            var compareResults = result.Data.SelectToken("$.result")?.ToObject<CompareResults>();

            Assert.IsNotNull(result);
            Assert.IsNotNull(compareResults);
            Assert.AreEqual(different, compareResults?.ContainsIsDifferenceResult());
        }

        [Test]
        public void CanHaveMultipleTokensForCompareResults()
        {
            //Arrange
            var data = JToken.Parse(
                "{\"result\":[{},{}], \"rootObject\" : {\"firstProperty\":1,\"secondObjectProperty\":{\"secondValue\":2},\"ArrayProperty\":[1,{\"arrayObjectItemProperty\":4}]}}");

            var sut = new Compare
            {
                FirstPath = "$.rootObject",
                SecondPath = "$.rootObject",
                ResultPath = "$.result[*]",
                Settings = new CompareSettings()
            };

            //Act
            var result = sut.Execute(data, executeOptions);

            //Assert
            Assert.That(result.Data.SelectToken("$.result[0]")?.ToString(), Is.EqualTo(
                result.Data.SelectToken("$.result[1]")?.ToString()));
            Assert.IsTrue(result.Data.SelectToken("$.result[0]")?.Any());
        }

        [Test]
        public void CanUseFluentApi()
        {
            var script = new JLioScript()
                    .Compare("$.first")
                    .With("$.second")
                    .UsingDefaultSettings()
                    .SetResultOn("$.result")
                ;
            var result = script.Execute(JToken.Parse("{\"first\":true,\"second\":true}"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.result")?.Type, JTokenType.Null);
        }

        [Test]
        public void CanUseFilteringResultApi()
        {
            var settings = new CompareSettings
            {
                ResultTypes = new List<DifferenceType>
                {
                    DifferenceType.TypeDifference
                }
            };

            var script = new JLioScript()
                    .Compare("$.first")
                    .With("$.second")
                    .Using(settings)
                    .SetResultOn("$.result")
                ;
            var result = script.Execute(JToken.Parse("{\"first\":true,\"second\":true}"));
            var compareResults = result.Data.SelectToken("$.result")?.ToObject<CompareResults>();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.result")?.Type, JTokenType.Null);
            Assert.IsNotNull(compareResults);
            Assert.IsFalse(compareResults.All(r => settings.ResultTypes.Contains(r.DifferenceType)));
        }
    }
}