using System.Collections.Generic;
using System.Linq;
using JLio.Commands;
using JLio.Commands.Builders;
using JLio.Core;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace JLio.UnitTests.CommandsTests
{
   public class MergeTests
    {

        private JLioExecutionOptions executeOptions;

        [SetUp]
        public void Setup()
        {
            executeOptions = JLioExecutionOptions.CreateDefault();
        }
            [Test]
        public void MergeTwoComplexArraysUnique()
        {
            var expectedResult =
                "{\"first\": [  {    \"item\": \"1\"  },  {    \"item\": 2  },  {    \"item\": 3.1  }],\"second\": [  {    \"item\": \"4\"  },  {    \"item\": 5  },  {    \"item\": 3.1  },  {    \"item\": \"1\"  },  {    \"item\": 2  }]}";
            var data =
                JObject.Parse(
                    "{\"first\":[{\"item\":\"1\"},{\"item\":2},{\"item\":3.1}],\"second\":[{\"item\":\"4\"},{\"item\":5},{\"item\":3.1}]}");

            string path = "$.first";
            string targetPath = "$.second";
            var mergeSettings = new MergeSettings
            {
                ArraySettings = new List<MergeArraySettings>
                        {new MergeArraySettings {ArrayPath = "$.second", UniqueItemsWithoutKeys = true}}
            };

            var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
        }

    }
}
