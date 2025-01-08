using Newtonsoft.Json.Linq;

namespace JLio.UnitTests.CommandsTestV2.PutTests
{

    public class SetTestCase : TestCase
    {
        public JToken Data { get; set; }
        public string Path { get; set; }
        public JToken Value { get; set; }
        public bool ExpectedSuccess { get; set; }
        public JToken ExpectedData { get; set; }
    }

}
