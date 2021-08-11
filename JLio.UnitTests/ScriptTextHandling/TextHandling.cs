using System;
using JLio.Client;
using JLio.Core;
using Newtonsoft.Json;
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
            var typedScript = JLioScript.Parse(script, options);
            var scriptText = JsonConvert.SerializeObject(typedScript, options.JLioFunctionConverter);
        }
    }
}