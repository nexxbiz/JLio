using JLio.Client;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]",
            "{\"myObject\":{\"initialProperty\":\"initial value\"}}")]
        public void Test1(string scriptText, string data)
        {
            var script = JLioConvert.Parse(scriptText);
            var result = script.Execute(JToken.Parse(data));

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
        }
    }
}