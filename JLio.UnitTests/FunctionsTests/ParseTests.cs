﻿using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    public class ParseTests
    {
        private ExecutionOptions executeOptions;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeOptions = ExecutionOptions.CreateDefault();
        }

        [TestCase("=parse()", "{\"result\" : \"3\" }", 3)]
        [TestCase("=parse()", "{\"result\" : \"\\\"3\\\"\"}", "\"3\"")]
        [TestCase("=parse()", "{\"result\" : \"{\\\"demo\\\":67}\"}", "{\"demo\":67}")]
        public void scriptTestSet(string function, string data, object expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult.ToString() ?? string.Empty),
                result.Data.SelectToken("$.result")));
        }

        [TestCase("=parse($.item)", "{\"item\" : \"3\" }", 3)]
        [TestCase("=parse($.item)", "{\"item\" : \"\\\"3\\\"\"}", "3")]
        [TestCase("=parse($.item)", "{\"item\" : \"{\\\"demo\\\":67}\"}", "{\"demo\": 67}")]
        public void scriptTestAdd(string function, string data, object expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsNotNull(result.Data.SelectToken("$.result"));
        }

        [TestCase("=parse($.item)", "{\"result\" : \"{\\\"demo :67}\"}")]
        [TestCase("=parse()", "{\"result\" : 3 }")]
        public void scriptTestAddFaultyString(string function, string data)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeOptions);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
        }

        [Test]
        public void CanbeUsedInFluentApi()
        {
            var script = new JLioScript()
                    .Set(new Parse())
                    .OnPath("$.id")
                    .Add(new Parse("$.id"))
                    .OnPath("$.result")
                ;
            var result = script.Execute(JObject.Parse("{\"id\" : \"3\" }"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreNotEqual(result.Data.SelectToken("$.result")?.Type, JTokenType.Null);
            Assert.AreEqual(result.Data.SelectToken("$.result").Value<int>(), 3);
        }
    }
}