using System;
using JLio.Client;
using JLio.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.ScriptTextHandling
{
    public class TextHandling
    {
        private JLioParseOptions options;

        [SetUp]
        public void Setup()
        {
            options = JLioParseOptions.CreateDefault();
        }

        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]",
        typeof(FixedValue))]
        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=datetime(UTC)\",\"command\":\"add\"}]",
        typeof(FixedValue))]
        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=datetime(datetime(UTC))\",\"command\":\"add\"}]",
        typeof(FixedValue))]
        [TestCase(
        "[{\"path\":\"$.myObject.newProperty\",\"value\":\"=concat('fixed', @.localPath, $. rootPath, datetime(UTC))\",\"command\":\"add\"}]",
        typeof(FixedValue))]
        public void Test1(string script, Type valueType)
        {
            var typedScript = JLioConvert.Parse(script, options);
            var scriptText = JsonConvert.SerializeObject(typedScript, options.JLioFunctionConverter);
        }

        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]")]
        [TestCase("[{\"path\": \"$.myObject.newProperty\",\"value\": 1,\"command\": \"add\"}]")]
        [TestCase("[{\"path\": \"$.myObject.newProperty\",\"value\": { \"new object\": \"Added by value\" },\"command\": \"add\"}]")]
        public void CanParse(string scriptText)
        {
            Assert.DoesNotThrow(() =>
            {
                JLioConvert.Parse(scriptText);
            }
            );
        }


        [TestCase("", "{\"myObject\":{\"initialProperty\":\"initial value\"}}")]
        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]",
           "{\"myObject\":{\"initialProperty\":\"initial value\"}}")]
        public void CanParseAndExecute(string scriptText, string data)
        {
            var script = JLioConvert.Parse(scriptText);
            var result = script.Execute(JToken.Parse(data));

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);
        }
    }
}