using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace JLio.Commands.Advanced.Settings
{
    public class MatchSettings
    {
        public bool HasKeys => KeyPaths.Any();

        [JsonProperty("keyPaths")]
        public List<string> KeyPaths { get; set; } = new List<string>();
    }
}