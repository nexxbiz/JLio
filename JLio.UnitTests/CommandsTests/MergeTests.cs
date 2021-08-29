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

        [Test]
        public void MergeTwoComplexArraysUniqueWithComplexKeys()
        {
            var expectedResult =
                "{\"first\":[{\"key\":{\"id\":\"1\",\"demo\":3,\"sub\":\"a\"},\"item\":\"1a\",\"valueFirst\":\"first id 1a\",\"valueCommon\":\"common first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueFirst\":\"first id 2a\",\"valueCommon\":\"common first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueFirst\":\"first id 1b\",\"valueCommon\":\"common first id 1b\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}],\"second\":[{\"key\":{\"id\":\"1\",\"sub\":\"a\",\"demo\":3},\"item\":\"1a\",\"valueSecond\":\"second id 1a\",\"valueCommon\":\"common first id 1a\",\"valueFirst\":\"first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueSecond\":\"second id 2a\",\"valueCommon\":\"common first id 2a\",\"valueFirst\":\"first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueSecond\":\"second id 1b\",\"valueCommon\":\"common first id 1b\",\"valueFirst\":\"first id 1b\"},{\"key\":{\"id\":\"3\",\"sub\":\"a\"},\"item\":\"3.1\",\"valueSecond\":\"second id 3a\",\"valueCommon\":\"common scond id 3a\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}]}";
            var data = JObject.Parse(
                "{\"first\":[{\"key\":{\"id\":\"1\", \"demo\": 3 ,\"sub\":\"a\"},\"item\":\"1a\",\"valueFirst\":\"first id 1a\",\"valueCommon\":\"common first id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":2,\"valueFirst\":\"first id 2a\",\"valueCommon\":\"common first id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueFirst\":\"first id 1b\",\"valueCommon\":\"common first id 1b\"},{\"key\":{\"id\":\"1\",\"sub\":\"c\"},\"item\":\"3.1\",\"valueFirst\":\"first id 1c\",\"valueCommon\":\"common first id 1c\"}],\"second\":[{\"key\":{\"id\":\"1\",\"sub\":\"a\"},\"item\":\"4\",\"valueSecond\":\"second id 1a\",\"valueCommon\":\"common scond id 1a\"},{\"key\":{\"id\":\"2\",\"sub\":\"a\"},\"item\":5,\"valueSecond\":\"second id 2a\",\"valueCommon\":\"common scond id 2a\"},{\"key\":{\"id\":\"1\",\"sub\":\"b\"},\"item\":3.1,\"valueSecond\":\"second id 1b\",\"valueCommon\":\"common scond id 1b\"},{\"key\":{\"id\":\"3\",\"sub\":\"a\"},\"item\":\"3.1\",\"valueSecond\":\"second id 3a\",\"valueCommon\":\"common scond id 3a\"}]}");


            string path = "$.first";
            string targetPath = "$.second";
            var mergeSettings = new MergeSettings
            {
                ArraySettings = new List<MergeArraySettings>
                    {
                        new MergeArraySettings
                            {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id", "key.sub"}}
                    }
            };

            var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);

            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
               
        }

        [Test]
        public void MergeTwoComplexArraysUniquewithKeys()
        {
            var expectedResult =
                "{\"first\": [  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueFirst\": \"first id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueFirst\": \"first id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }],\"second\": [  {    \"key\": {      \"id\": \"4\"    },    \"item\": \"4\",    \"valueSecond\": \"second id 4\",    \"valueCommon\": \"common scond id 4\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueSecond\": \"second id 2\",    \"valueCommon\": \"common first id 2\",    \"valueFirst\": \"first id 2\"  },  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueSecond\": \"second id 1\",    \"valueCommon\": \"common first id 1\",    \"valueFirst\": \"first id 1\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }]}";
            var data = JObject.Parse(
                "{\"first\":[{\"key\":{\"id\":\"1\"},\"item\":\"1\",\"valueFirst\":\"first id 1\",\"valueCommon\":\"common first id 1\"},{\"key\":{\"id\":\"2\"},\"item\":2,\"valueFirst\":\"first id 2\",\"valueCommon\":\"common first id 2\"},{\"key\":{\"id\":\"3\"},\"item\":3.1,\"valueFirst\":\"first id 3\",\"valueCommon\":\"common first id 3\"}],\"second\":[{\"key\":{\"id\":\"4\"},\"item\":\"4\",\"valueSecond\":\"second id 4\",\"valueCommon\":\"common scond id 4\"},{\"key\":{\"id\":\"2\"},\"item\":5,\"valueSecond\":\"second id 2\",\"valueCommon\":\"common scond id 2\"},{\"key\":{\"id\":\"1\"},\"item\":3.1,\"valueSecond\":\"second id 1\",\"valueCommon\":\"common scond id 1\"}]}");
           
            string path = "$.first";
            string targetPath = "$.second";
            var mergeSettings = new MergeSettings
            {
                ArraySettings = new List<MergeArraySettings>
                        {new MergeArraySettings {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id"}}}
            };
            var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
        }

        [Test]
        public void MergeTwoComplexArraysUniquewithKeysAndOnlyStructure()
        {
            var expectedResult =
                "{\"first\": [  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueFirst\": \"first id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueFirst\": \"first id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }],\"second\": [  {    \"key\": {      \"id\": \"4\"    },    \"item\": \"4\",    \"valueSecond\": \"second id 4\",    \"valueCommon\": \"common scond id 4\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 5,    \"valueSecond\": \"second id 2\",    \"valueCommon\": \"common scond id 2\",    \"valueFirst\": \"first id 2\"  },  {    \"key\": {      \"id\": \"1\"    },    \"item\": 3.1,    \"valueSecond\": \"second id 1\",    \"valueCommon\": \"common scond id 1\",    \"valueFirst\": \"first id 1\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }]}";
            var data = JObject.Parse(
                "{\"first\":[{\"key\":{\"id\":\"1\"},\"item\":\"1\",\"valueFirst\":\"first id 1\",\"valueCommon\":\"common first id 1\"},{\"key\":{\"id\":\"2\"},\"item\":2,\"valueFirst\":\"first id 2\",\"valueCommon\":\"common first id 2\"},{\"key\":{\"id\":\"3\"},\"item\":3.1,\"valueFirst\":\"first id 3\",\"valueCommon\":\"common first id 3\"}],\"second\":[{\"key\":{\"id\":\"4\"},\"item\":\"4\",\"valueSecond\":\"second id 4\",\"valueCommon\":\"common scond id 4\"},{\"key\":{\"id\":\"2\"},\"item\":5,\"valueSecond\":\"second id 2\",\"valueCommon\":\"common scond id 2\"},{\"key\":{\"id\":\"1\"},\"item\":3.1,\"valueSecond\":\"second id 1\",\"valueCommon\":\"common scond id 1\"}]}");
           
            string path = "$.first";
            string targetPath = "$.second";
            var mergeSettings = new MergeSettings
            {
                Strategy = MergeSettings.STRATEGY_ONLY_STRUCTURE,
                ArraySettings = new List<MergeArraySettings>
                        {new MergeArraySettings {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id"}}}
            };

            var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
        }

        [Test]
        public void MergeTwoComplexArraysUniquewithKeysAndOnlyValues()
        {
            var expectedResult =
                "{\"first\": [  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueFirst\": \"first id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueFirst\": \"first id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }],\"second\": [  {    \"key\": {      \"id\": \"4\"    },    \"item\": \"4\",    \"valueSecond\": \"second id 4\",    \"valueCommon\": \"common scond id 4\"  },  {    \"key\": {      \"id\": \"2\"    },    \"item\": 2,    \"valueSecond\": \"second id 2\",    \"valueCommon\": \"common first id 2\"  },  {    \"key\": {      \"id\": \"1\"    },    \"item\": \"1\",    \"valueSecond\": \"second id 1\",    \"valueCommon\": \"common first id 1\"  },  {    \"key\": {      \"id\": \"3\"    },    \"item\": 3.1,    \"valueFirst\": \"first id 3\",    \"valueCommon\": \"common first id 3\"  }]}";
            var data = JObject.Parse(
                "{\"first\":[{\"key\":{\"id\":\"1\"},\"item\":\"1\",\"valueFirst\":\"first id 1\",\"valueCommon\":\"common first id 1\"},{\"key\":{\"id\":\"2\"},\"item\":2,\"valueFirst\":\"first id 2\",\"valueCommon\":\"common first id 2\"},{\"key\":{\"id\":\"3\"},\"item\":3.1,\"valueFirst\":\"first id 3\",\"valueCommon\":\"common first id 3\"}],\"second\":[{\"key\":{\"id\":\"4\"},\"item\":\"4\",\"valueSecond\":\"second id 4\",\"valueCommon\":\"common scond id 4\"},{\"key\":{\"id\":\"2\"},\"item\":5,\"valueSecond\":\"second id 2\",\"valueCommon\":\"common scond id 2\"},{\"key\":{\"id\":\"1\"},\"item\":3.1,\"valueSecond\":\"second id 1\",\"valueCommon\":\"common scond id 1\"}]}");
           
            string path = "$.first";
            string targetPath = "$.second";
            var mergeSettings = new MergeSettings
            {
                    Strategy = MergeSettings.STRATEGY_ONLY_VALUES,
                    ArraySettings = new List<MergeArraySettings>
                        {new MergeArraySettings {ArrayPath = "$.second", KeyPaths = new List<string> {"key.id"}}}
                };

            var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
        }

        [Test]
        public void MergeTwoComplexArraysUniquewithKeysWithDoubleDotNotations()
        {
            var expectedResult =
                "{\"first\": [  {    \"PP\": {      \"PP_EXTERN\": 1,      \"PP_MYBRA\": \"extern 1\"    }  },  {    \"PP\": {      \"PP_EXTERN\": 2,      \"PP_MYBRA\": \"extern 2\"    }  }],\"second\": {  \"PP\": {    \"PP_EXTERN\": 1,    \"PP_MYBRA\": \"extern 1\"  },  \"previous\": {    \"PP\": {      \"PP_EXTERN\": 2,      \"PP_MYBRA\": \"extern 2\"    }  }}}";
            var data = JObject.Parse(
                "{\"first\":[{\"PP\":{\"PP_EXTERN\":1,\"PP_MYBRA\":\"extern 1\"}},{\"PP\":{\"PP_EXTERN\":2,\"PP_MYBRA\":\"extern 2\"}}],\"second\":{\"PP\":{\"PP_EXTERN\":1},\"previous\":{\"PP\":{\"PP_EXTERN\":2}}}}");

            string path = "$.first..PP";
            string targetPath = "$.second..PP";
            var mergeSettings = new MergeSettings
                    {
                        MatchSettings = new MatchSettings
                        {
                            KeyPaths = new List<string> { "PP_EXTERN" }
                        }
                    };

            var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));

        }

        [Test]
        public void MergeTwoComplexPropertiesInObject()
        {
            var expectedResult =
                "{\"first\": {  \"common\": 4,  \"onlyFirst\": \"first value\"},\"second\": {  \"common\": 4,  \"onlySecond\": \"second value\",  \"onlyFirst\": \"first value\"}}";
            var data =
                JObject.Parse(
                    "{\"first\":{\"common\":4,\"onlyFirst\":\"first value\"},\"second\":{\"common\":5,\"onlySecond\":\"second value\"}}");
            
            
            string path = "$.first";
            string targetPath = "$.second";

            var result = new Merge(path, targetPath).Execute(data, executeOptions);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
        }

        [Test]
        public void MergeTwoComplexPropertiesInObjectOnlyStructure()
        {
            var expectedResult =
                "{\"first\": {  \"common\": 4,  \"onlyFirst\": \"first value\"},\"second\": {  \"common\": 5,  \"onlySecond\": \"second value\",  \"onlyFirst\": \"first value\"}}";
            var data =
                JObject.Parse(
                    "{\"first\":{\"common\":4,\"onlyFirst\":\"first value\"},\"second\":{\"common\":5,\"onlySecond\":\"second value\"}}");
            string path = "$.first";
            string targetPath = "$.second";
            var mergeSettings = new MergeSettings { Strategy = MergeSettings.STRATEGY_ONLY_STRUCTURE };
           
            var result = new Merge(path, targetPath,mergeSettings).Execute(data, executeOptions);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
        }

        [Test]
        public void MergeTwoComplexPropertiesInObjectOnlyValues()
        {
            var expectedResult =
                "{\"first\": {  \"common\": 4,  \"onlyFirst\": \"first value\"},\"second\": {  \"common\": 4,  \"onlySecond\": \"second value\"}}";
            var data =
                JObject.Parse(
                    "{\"first\":{\"common\":4,\"onlyFirst\":\"first value\"},\"second\":{\"common\":5,\"onlySecond\":\"second value\"}}");
           
            string path = "$.first";
            string targetPath = "$.second";
            var mergeSettings = new MergeSettings { Strategy = MergeSettings.STRATEGY_ONLY_VALUES };

            var result = new Merge(path, targetPath, mergeSettings).Execute(data, executeOptions);
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(expectedResult), result.Data));
        }
    }
}
