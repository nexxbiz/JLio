using System.Collections.Generic;
using JLio.Commands.Advanced.Models;
using Newtonsoft.Json;

namespace JLio.Commands.Advanced.Settings
{
    public class CompareSettings
    {
        [JsonProperty("arraySettings")]
        public List<CompareArraySettings> ArraySettings { get; set; } = new List<CompareArraySettings>();

        [JsonProperty("resultTypes")]
        public List<DifferenceType> ResultTypes { get; set; } = new List<DifferenceType>();

        public static CompareSettings CreateDefault()
        {
            return new CompareSettings();
        }
    }
}