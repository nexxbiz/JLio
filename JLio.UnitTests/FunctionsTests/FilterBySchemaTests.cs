using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JLio.Client;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using JLio.Functions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;

namespace JLio.UnitTests.FunctionsTests
{
    [TestFixture]
    public class FilterBySchemaTests
    {
        private IExecutionContext executeContext;
        private ParseOptions parseOptions;

        [SetUp]
        public void Setup()
        {
            parseOptions = ParseOptions.CreateDefault();
            executeContext = ExecutionContext.CreateDefault();
        }
        
        [Test]
        [TestCase("{\"type\":\"object\",\"description\":\"The point of interest location description.\",\"properties\":{\"pageIndex\":{\"type\":\"number\",\"description\":\"The page index to view\"},\"points\":{\"description\":\"Used if viewState.zoomWidth is true; specifies 4 corners of the rectangle to make visible.\",\"type\":\"string\"},\"something\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"loc\":{\"type\":\"string\"},\"toll\":{\"type\":\"string\"},\"message\":{\"type\":\"string\"}},\"required\":[\"loc\"]}},\"viewState\":{\"type\":\"object\",\"description\":\"The view state information\",\"properties\":{\"hasViewState\":{\"type\":\"boolean\",\"description\":\"Whether there is view state information to use\"},\"rotate\":{\"type\":\"number\",\"description\":\"Degrees of rotation\"},\"extents\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit extents or not\"},\"zoomWidth\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit width or not\"},\"scaleFactor\":{\"type\":\"number\",\"description\":\"The scale factor for the view state\"},\"deviceRect\":{\"type\":\"object\",\"description\":\"The device rectangle geometry\",\"properties\":{\"left\":{\"type\":\"number\",\"description\":\"The left edge of the device rectangle\"},\"right\":{\"type\":\"number\",\"description\":\"The right edge of the device rectangle\"},\"top\":{\"type\":\"number\",\"description\":\"The top edge of the device rectangle\"},\"bottom\":{\"type\":\"number\",\"description\":\"The bottom edge of the device rectangle\"}}},\"eyePoint\":{\"type\":\"object\",\"description\":\"The eyepoint coordinate\",\"properties\":{\"x\":{\"type\":\"number\",\"description\":\"The x coordinate of the eyepoint\"},\"y\":{\"type\":\"number\",\"description\":\"The y coordinate of the eyepoint\"}}}}}}}")]
        public void CanBeUsedInFluentApi(string schema)
        {
            var parsedSchema = JSchema.Parse(schema);

            var script = new JLioScript()
                    .Set(new FilterBySchema(parsedSchema))
                    .OnPath("$.demo");
            var result = script.Execute(JToken.Parse("{\"demo\":{\"points\":\"dolor Excepteur aliquip in\",\"viewState\":{\"sintc\":false,\"velit_c3\":\"dolore Ut\",\"aliquab84\":98258463,\"tempor_0\":true,\"amet_54\":true,\"sunt_c\":\"exercitation Excepteur ipsum\",\"ex_16\":9425019.714207217,\"do_11e\":false},\"something\":[{\"loc\":\"dolor et Excepteur exercitation\",\"toll\":\"ad elit esse adipisicing\",\"message\":\"tempor est\"},{\"loc\":\"veniam do fugiat labore eiusmod\",\"toll\":\"dolore aliqua veniam sit\",\"message\":\"aliquip ad do irure velit\"},{\"loc\":\"et\"},{\"loc\":\"dolore non dolore sint\",\"toll\":\"minim ut consectetur fugiat\",\"message\":\"cillum exercitation quis id laborum\"}]  }}"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }
        
        [Test]
        public void CanBeUsedInFluentApi_SchemaPath()
        {
            var script = new JLioScript()
                    .Set(new FilterBySchema("$.schema"))
                    .OnPath("$.demo");
            var result = script.Execute(JToken.Parse("{\"schema\":{\"type\":\"object\",\"description\":\"The point of interest location description.\",\"properties\":{\"pageIndex\":{\"type\":\"number\",\"description\":\"The page index to view\"},\"points\":{\"description\":\"Used if viewState.zoomWidth is true; specifies 4 corners of the rectangle to make visible.\",\"type\":\"string\"},\"something\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"loc\":{\"type\":\"string\"},\"toll\":{\"type\":\"string\"},\"message\":{\"type\":\"string\"}},\"required\":[\"loc\"]}},\"viewState\":{\"type\":\"object\",\"description\":\"The view state information\",\"properties\":{\"hasViewState\":{\"type\":\"boolean\",\"description\":\"Whether there is view state information to use\"},\"rotate\":{\"type\":\"number\",\"description\":\"Degrees of rotation\"},\"extents\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit extents or not\"},\"zoomWidth\":{\"type\":\"boolean\",\"description\":\"Whether the view should scale to fit width or not\"},\"scaleFactor\":{\"type\":\"number\",\"description\":\"The scale factor for the view state\"},\"deviceRect\":{\"type\":\"object\",\"description\":\"The device rectangle geometry\",\"properties\":{\"left\":{\"type\":\"number\",\"description\":\"The left edge of the device rectangle\"},\"right\":{\"type\":\"number\",\"description\":\"The right edge of the device rectangle\"},\"top\":{\"type\":\"number\",\"description\":\"The top edge of the device rectangle\"},\"bottom\":{\"type\":\"number\",\"description\":\"The bottom edge of the device rectangle\"}}},\"eyePoint\":{\"type\":\"object\",\"description\":\"The eyepoint coordinate\",\"properties\":{\"x\":{\"type\":\"number\",\"description\":\"The x coordinate of the eyepoint\"},\"y\":{\"type\":\"number\",\"description\":\"The y coordinate of the eyepoint\"}}}}}}},\"demo\":{\"pageIndex\":5,\"shouldBeRemoved\":true}}"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }
        
        [Test]
        [TestCase("=filterBySchema()", "{\"result\" : [1,2]}")]
        public void WillReturnErrorFalse(string function, string data)
        {
            var script = $"[{{\"path\":\"$.result[*]\",\"value\":\"{function}\",\"command\":\"set\"}}]";
            var result = JLioConvert.Parse(script, parseOptions).Execute(JToken.Parse(data), executeContext);

            Assert.IsTrue(executeContext.Logger.LogEntries.Any(i => i.Level == LogLevel.Error));
        }
    }
}