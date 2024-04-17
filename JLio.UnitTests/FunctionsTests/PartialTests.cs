using System.Linq;
using JLio.Client;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
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

        [TestCase("=partial(@.a)", "{\"result\":{\"a\":{\"b\":1},\"b\":1}}",
            "{\"a\":{\"b\":1}}")]
        [TestCase("=partial(@.a, @.c.d)", "{\"result\":{\"a\":1,\"b\":[1,2,3],\"c\":{\"d\":5,\"e\":[4,5,6]}}}",
            "{\"a\":1,\"c\":{\"d\":5}}")]
        [TestCase("=partial(@.a, @.c.d)", "{\"result\":{\"a\":[1,2],\"b\":[1,2,3],\"c\":{\"d\":[4,5],\"e\":[4,5,6]}}}",
            "{\"a\":[1,2],\"c\":{\"d\":[4,5]}}")]
        [TestCase("=partial(@.a[0], @.c.d)",
            "{\"result\":{\"a\":[1,2],\"b\":[1,2,3],\"c\":{\"d\":[4,5],\"e\":[4,5,6]}}}",
            "{\"a\":[1],\"c\":{\"d\":[4,5]}}")]
        [TestCase("=partial(@.a[?(@.b == true)].d.e)",
            "{\"result\":{\"a\":[{\"b\":true,\"c\":\"gone\",\"d\":{\"e\":\"stay\",\"f\":\"gone\"}},{\"b\":false,\"c\":\"gone\",\"d\":{\"e\":\"gone\",\"f\":\"gone\"}}]}}",
            "{\"a\":[{\"d\":{\"e\":\"stay\"}}]}")]
        [TestCase("=partial(@.a[?(@.b == true)].d.e, @.a[?(@.b == true)].c)",
            "{\"result\":{\"a\":[{\"b\":true,\"c\":\"stay\",\"d\":{\"e\":\"stay\",\"f\":\"gone\"}},{\"b\":false,\"c\":\"gone\",\"d\":{\"e\":\"gone\",\"f\":\"gone\"}}]}}",
            "{\"a\":[{\"d\":{\"e\":\"stay\"},\"c\":\"stay\"}]}")]
        public void PartialSet(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [TestCase("=partial($.source,@.a, @.c.d)", "{\"source\":{\"a\":1,\"b\":[1,2,3],\"c\":{\"d\":5,\"e\":[4,5,6]}}}",
            "{\"a\":1,\"c\":{\"d\":5}}")]
        [TestCase("=partial($.source,@.a, @.c.d)",
            "{\"source\":{\"a\":[1,2],\"b\":[1,2,3],\"c\":{\"d\":[4,5],\"e\":[4,5,6]}}}",
            "{\"a\":[1,2],\"c\":{\"d\":[4,5]}}")]
        [TestCase("=partial($.source,@.a[0], @.c.d)",
            "{\"source\":{\"a\":[1,2],\"b\":[1,2,3],\"c\":{\"d\":[4,5],\"e\":[4,5,6]}}}",
            "{\"a\":[1],\"c\":{\"d\":[4,5]}}")]
        [TestCase("=partial($.source,@.a[?(@.b == true)].d.e)",
            "{\"source\":{\"a\":[{\"b\":true,\"c\":\"gone\",\"d\":{\"e\":\"stay\",\"f\":\"gone\"}},{\"b\":false,\"c\":\"gone\",\"d\":{\"e\":\"gone\",\"f\":\"gone\"}}]}}",
            "{\"a\":[{\"d\":{\"e\":\"stay\"}}]}")]
        [TestCase("=partial($.source,@.a[?(@.b == true)].d.e, @.a[?(@.b == true)].c)",
            "{\"source\":{\"a\":[{\"b\":true,\"c\":\"stay\",\"d\":{\"e\":\"stay\",\"f\":\"gone\"}},{\"b\":false,\"c\":\"gone\",\"d\":{\"e\":\"gone\",\"f\":\"gone\"}}]}}",
            "{\"a\":[{\"d\":{\"e\":\"stay\"},\"c\":\"stay\"}]}")]
        public void PartialAdd(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"add\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [TestCase("=partial($.source,@.a, @.c.d)",
            "{\"result\":1,\"source\":{\"a\":1,\"b\":[1,2,3],\"c\":{\"d\":5,\"e\":[4,5,6]}}}",
            "{\"a\":1,\"c\":{\"d\":5}}")]
        [TestCase("=partial($.source,@.a, @.c.d)",
            "{\"result\":1,\"source\":{\"a\":[1,2],\"b\":[1,2,3],\"c\":{\"d\":[4,5],\"e\":[4,5,6]}}}",
            "{\"a\":[1,2],\"c\":{\"d\":[4,5]}}")]
        [TestCase("=partial($.source,@.a[0], @.c.d)",
            "{\"result\":1,\"source\":{\"a\":[1,2],\"b\":[1,2,3],\"c\":{\"d\":[4,5],\"e\":[4,5,6]}}}",
            "{\"a\":[1],\"c\":{\"d\":[4,5]}}")]
        [TestCase("=partial($.source,@.a[?(@.b == true)].d.e)",
            "{\"result\":1,\"source\":{\"a\":[{\"b\":true,\"c\":\"gone\",\"d\":{\"e\":\"stay\",\"f\":\"gone\"}},{\"b\":false,\"c\":\"gone\",\"d\":{\"e\":\"gone\",\"f\":\"gone\"}}]}}",
            "{\"a\":[{\"d\":{\"e\":\"stay\"}}]}")]
        [TestCase("=partial($.source,@.a[?(@.b == true)].d.e, @.a[?(@.b == true)].c)",
            "{\"result\":1,\"source\":{\"a\":[{\"b\":true,\"c\":\"stay\",\"d\":{\"e\":\"stay\",\"f\":\"gone\"}},{\"b\":false,\"c\":\"gone\",\"d\":{\"e\":\"gone\",\"f\":\"gone\"}}]}}",
            "{\"a\":[{\"d\":{\"e\":\"stay\"},\"c\":\"stay\"}]}")]
        public void PartialSetWithDifferentSource(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [TestCase("=partial()", "{\"result\" : 2}")]
        [TestCase("=partial($.source[*], @.a)",
            "{\"result\":{\"a\":[{\"b\":true,\"c\":\"gone\",\"d\":{\"e\":\"stay\",\"f\":\"gone\"}},{\"b\":false,\"c\":\"gone\",\"d\":{\"e\":\"gone\",\"f\":\"gone\"}}]}}")]
        public void WillReturnError(string function, string data)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
        }

        [TestCase("=partial(@.z, @.c.d, @.c.z)", "{\"result\":{\"a\":1,\"b\":[1,2,3],\"c\":{\"d\":5,\"e\":[4,5,6]}}}",
       "{\"c\":{\"d\":5}}")]
        public void CanHandleNonExisitedPaths(string function, string data, string expectedResult)
        {
            var script = $"[{{\"path\":\"$.result\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(executeContext.Logger.LogEntries.TrueForAll(i => i.Level != LogLevel.Error));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedResult), result.Data.SelectToken("$.result")));
        }

        [Test]
        public void CanBeUsedInFluentApi()
        {
            var script = new JLioScript()
                .Set(new Partial("@.a", "@.c.d"))
                .OnPath("$.result");
            var result =
                script.Execute(JToken.Parse("{\"result\":{\"a\":1,\"b\":[1,2,3],\"c\":{\"d\":5,\"e\":[4,5,6]}}}"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }
    }
}