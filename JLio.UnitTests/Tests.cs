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
        public void Test1(string script, string data)
        {
            var commands = JLioScript.Parse(script);
            var result = commands.Execute(JToken.Parse(data));

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
        }
    }
}