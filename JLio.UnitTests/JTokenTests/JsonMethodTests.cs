using System.Collections.Generic;
using System.Linq;
using JLio.Core.Extensions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.JTokenTests
{
    public class JTokenTests
    {
        [Test]
        public void ConvertFromDictionary()
        {
            var source = new Dictionary<string, JToken>
            {
                {"item1", new JValue(1)},
                {"item2", new JValue(2)}
            };
            var sut = source.ConvertToDataObject();
            Assert.AreEqual(1, sut.SelectToken("$.item1")?.Value<int>());
            Assert.AreEqual(2, sut.SelectToken("$.item2")?.Value<int>());
        }

        [Test]
        public void ConvertToDictionary()
        {
            var source = JObject.Parse("{\"item1\":1,\"item2\":2}");
            var sut = source.ConvertToDictionary();
            Assert.IsTrue(sut.ContainsKey("item1"));
            Assert.IsTrue(sut.ContainsKey("item2"));
            Assert.AreEqual(1, sut["item1"].Value<int>());
            Assert.AreEqual(2, sut["item2"].Value<int>());
        }

        [Test]
        public void ConvertToDictionaryWithFilter()
        {
            var source = JObject.Parse("{\"item1\":1,\"item2\":2}");
            var sut = source.ConvertToDictionary(new List<string> {"item1"});
            Assert.IsTrue(sut.ContainsKey("item1"));
            Assert.IsFalse(sut.ContainsKey("item2"));
            Assert.AreEqual(1, sut["item1"].Value<int>());
        }

        [TestCase("$.item[*]", "{\"item\":[1,\"a\"]}", false)]
        [TestCase("$.item[*]", "{\"item\":[1,2]}", true)]
        public void SelectSameTokenTypes(string path, string jObject, bool sameTypes)
        {
            var source = JObject.Parse(jObject);
            var sut = new JsonPathItemsFetcher().SelectTokens("$.item[*]", source);

            Assert.AreEqual(sameTypes, sut.AreSameTokenTypes);
        }

        [TestCase("$.item[*]", "{\"item\":[1,\"a\"]}", JTokenType.Integer, 1)]
        [TestCase("$.item[*]", "{\"item\":[1,2]}", JTokenType.Integer, 2)]
        public void SelectByJTokenType(string path, string jObject, JTokenType type, int numberOfItems)
        {
            var source = JObject.Parse(jObject);
            var sut = new JsonPathItemsFetcher().SelectTokens("$.item[*]", source);

            Assert.AreEqual(numberOfItems, sut.GetTokens(type).Count());
        }
    }
}