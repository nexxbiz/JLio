using System;
using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    public class NewGuidTests
    {
        private JLioExecutionOptions executeOptions;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeOptions = JLioExecutionOptions.CreateDefault();
        }

        [TestCase("=newGuid()", "{}")]
        [TestCase("=newGuid(32)", "{}")]
        public void scriptTest(string function, string data)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
            Assert.DoesNotThrow(() => { Guid.Parse(result.Data.SelectToken("$.result").ToString()); });
        }

        [Test]
        public void CanbeUsedInFluentApi()
        {
            var script = new JLioScript()
                    .Add(new NewGuid())
                    .OnPath("$.id")
                ;
            var result = script.Execute(new JObject());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.id").Type, JTokenType.Null);
        }
    }
}