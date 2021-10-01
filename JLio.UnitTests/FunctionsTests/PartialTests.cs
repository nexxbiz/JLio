﻿using JLio.Client;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    public class PartialTests
    {
        private IExecutionContext executeContext;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeContext = ExecutionContext.CreateDefault();
        }

        [TestCase("=partial(@.a, @.c.d)", "{\"result\":{\"a\":1,\"b\":[1,2,3],\"c\":{\"d\":5,\"e\":[4,5,6]}}}",
            "{\"a\":1,\"c\":{\"d\":5}}")]
        public void PartialSetWithOnePath(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }
    }
}