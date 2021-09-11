using System.Linq;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests
{
    public class CopyMoveTests
    {
        private JToken data;
        private ExecutionOptions executeOptions;

        [SetUp]
        public void Setup()
        {
            executeOptions = ExecutionOptions.CreateDefault();
            data = JToken.Parse(
                "{ \"myString\": \"demo2\", \"myNumber\": 2.2, \"myInteger\": 20, \"myObject\": { \"myObject\": {\"myArray\": [ 2, 20, 200, 2000 ]}, \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ], \"myBoolean\": true, \"myNull\": null}");
        }

        [TestCase("$.myString", "$.myNewObject.newItem", "'demo2'")]
        [TestCase("$.myNumber", "$.myNewObject.newItem", "2.2")]
        [TestCase("$.myInteger", "$.myNewObject.newItem", "20")]
        [TestCase("$.myObject", "$.myNewObject.newItem",
            "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
        [TestCase("$.myArray", "$.myNewObject.newItem", "[ 2, 20, 200, 2000 ]")]
        [TestCase("$.myBoolean", "$.myNewObject.newItem", "true")]
        [TestCase("$.myNull", "$.myNewObject.newItem", "null")]
        [TestCase("$.myString", "$.myObject", "'demo2'")]
        [TestCase("$.myNumber", "$.myObject", "2.2")]
        [TestCase("$.myInteger", "$.myObject", "20")]
        [TestCase("$.myObject", "$.myObject",
            "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
        [TestCase("$.myArray", "$.myObject", "[ 2, 20, 200, 2000 ]")]
        [TestCase("$.myBoolean", "$.myObject", "true")]
        [TestCase("$.myNull", "$.myObject", "null")]
        public void PropertyCopyTests(string from, string to, string expectedValueToPath)
        {
            var result = new Copy(from, to).Execute(data, executeOptions);


            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
            Assert.IsTrue(JToken.DeepEquals(data.SelectToken(from), data.SelectToken(to)));
        }

        [TestCase("$.myString", "$.myArray", "[ 2, 20, 200, 2000, 'demo2' ]")]
        [TestCase("$.myNumber", "$.myArray", "[ 2, 20, 200, 2000, 2.2]")]
        [TestCase("$.myInteger", "$.myArray", "[ 2, 20, 200, 2000, 20]")]
        [TestCase("$.myObject", "$.myArray",
            "[ 2, 20, 200, 2000, { \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }]")]
        [TestCase("$.myArray[*]", "$.myArray", "[ 2, 20, 200, 2000 ,2, 20, 200, 2000 ]")]
        [TestCase("$.myArray[?(@ > 20)]", "$.myArray", "[ 2, 20, 200, 2000 , 200, 2000 ]")]
        [TestCase("$.myArray", "$.myArray", "[ 2, 20, 200, 2000 ,[2, 20, 200, 2000] ]")]
        [TestCase("$.myBoolean", "$.myArray", "[ 2, 20, 200, 2000, true]")]
        [TestCase("$.myNull", "$.myArray", "[ 2, 20, 200, 2000, null]")]
        public void PropertyCopyArrayTests(string from, string to, string expectedValueToPath)
        {
            var result = new Copy(from, to).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
        }

        [TestCase("$.myString", "$.myArray", "[ 2, 20, 200, 2000, 'demo2' ]")]
        [TestCase("$.myNumber", "$.myArray", "[ 2, 20, 200, 2000, 2.2]")]
        [TestCase("$.myInteger", "$.myArray", "[ 2, 20, 200, 2000, 20]")]
        [TestCase("$.myObject", "$.myArray",
            "[ 2, 20, 200, 2000, { \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }]")]
        [TestCase("$.myArray[*]", "$.myArray", "[ 2, 20, 200, 2000 ]")]
        [TestCase("$.myObject.myArray[?(@ > 20)]", "$.myArray", "[ 2, 20, 200, 2000 , 200, 2000 ]")]
        [TestCase("$.myBoolean", "$.myArray", "[ 2, 20, 200, 2000, true]")]
        [TestCase("$.myNull", "$.myArray", "[ 2, 20, 200, 2000, null]")]
        public void PropertyMoveArrayTests(string from, string to, string expectedValueToPath)
        {
            var result = new Move(from, to).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
        }

        [TestCase("$.myString", "$.myNewObject.newItem", "'demo2'")]
        [TestCase("$.myNumber", "$.myNewObject.newItem", "2.2")]
        [TestCase("$.myInteger", "$.myNewObject.newItem", "20")]
        [TestCase("$.myObject", "$.myNewObject.newItem",
            "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
        [TestCase("$.myArray", "$.myNewObject.newItem", "[ 2, 20, 200, 2000 ]")]
        [TestCase("$.myBoolean", "$.myNewObject.newItem", "true")]
        [TestCase("$.myNull", "$.myNewObject.newItem", "null")]
        [TestCase("$.myString", "$.myObject", "'demo2'")]
        [TestCase("$.myNumber", "$.myObject", "2.2")]
        [TestCase("$.myInteger", "$.myObject", "20")]
        [TestCase("$.myObject", "$.myObject",
            "{ \"myObject\": { \"myArray\": [ 2, 20, 200, 2000 ] }, \"myArray\": [ 2, 20, 200, 2000 ] }")]
        [TestCase("$.myArray", "$.myObject", "[ 2, 20, 200, 2000 ]")]
        [TestCase("$.myBoolean", "$.myObject", "true")]
        [TestCase("$.myNull", "$.myObject", "null")]
        public void PropertyMoveTests(string from, string to, string expectedValueToPath)
        {
            var result = new Move(from, to).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(expectedValueToPath), data.SelectToken(to)));
            Assert.IsTrue(from == to || data.SelectToken(from) == null);
        }

        [Test]
        public void CanUseFluentApi()
        {
            var data = JObject.Parse("{ \"demo\" : \"item\" }");

            var script = new JLioScript()
                    .Copy("$.demo")
                    .To("$.copiedDemo")
                    .Move("$.copiedDemo")
                    .To("$.result")
                ;
            var result = script.Execute(data);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(JToken.DeepEquals(result.Data.SelectToken("$.demo"), result.Data.SelectToken("$.result")));
            Assert.IsNull(result.Data.SelectToken("$.copiedDemo"));
        }

        [Test]
        public void CanExecuteCopyWithoutParametersSet()
        {
            var command = new Copy();
            var result = command.Execute(JToken.Parse("{\"first\":true,\"second\":true}"), executeOptions);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.Any());
        }

        [Test]
        public void CanExecuteMoveWithoutParametersSet()
        {
            var command = new Move();
            var result = command.Execute(JToken.Parse("{\"first\":true,\"second\":true}"), executeOptions);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(executeOptions.Logger.LogEntries.Any());
        }
    }
}