using System;
using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Models;
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

        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]")]
        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=datetime(UTC)\",\"command\":\"add\"}]")]
        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=datetime(datetime(UTC))\",\"command\":\"add\"}]")]
        [TestCase("[{\"path\":\"$.myObject.newProperty\",\"value\":\"=concat('fixed', @.localPath, $.rootPath, datetime(UTC))\",\"command\":\"add\"}]")]
        [TestCase("[{\"path\": \"$.myObject.newProperty\",\"value\": { \"new object\": \"Added by value\" },\"command\": \"add\"}]")]
        public void CanParseAndSerializeScript(string script)
        {
            var scriptText2 = JLioConvert.Serialize(JLioConvert.Parse(script));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(script), JToken.Parse(scriptText2)));
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

        [Test]
        public void CanSerializeAndDeserializeScriptForCommands()
        {
            var script = new JLioScript()
                .Add(new JValue(0)).OnPath("$.demo")
                .Set(new JValue(1)).OnPath("$.demo")
                .Move("$.demo").To("$.otherDemo")
                .Copy("$.otherDemo").To(" $.demo")
                .Remove("$.demo");
            var scriptText = string.Empty;
            Assert.DoesNotThrow(() =>
            {
                scriptText = JLioConvert.Serialize(script);
            });
            Assert.IsFalse(string.IsNullOrEmpty(scriptText));
            var scriptText2 = JLioConvert.Serialize(JLioConvert.Parse(scriptText));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(scriptText), JToken.Parse(scriptText2)));
        }

    }
}