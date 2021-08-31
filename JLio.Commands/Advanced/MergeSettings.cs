using System.Collections.Generic;
using Newtonsoft.Json;

namespace JLio.Commands.Advanced
{
    public class MergeSettings
    {
        public const string STRATEGY_FULLMERGE = "fullMerge";
        public const string STRATEGY_ONLY_STRUCTURE = "onlyStructure";
        public const string STRATEGY_ONLY_VALUES = "onlyValues";

        [JsonProperty("arraySettings")]
        public List<MergeArraySettings> ArraySettings { get; set; } = new List<MergeArraySettings>();

        [JsonProperty("matchSettings")]
        public MatchSettings MatchSettings { get; set; } = new MatchSettings();

        [JsonProperty("strategy")]
        public string Strategy { get; set; } = STRATEGY_FULLMERGE;

        public static MergeSettings CreateDefault()
        {
            return new MergeSettings();
        }
    }
}