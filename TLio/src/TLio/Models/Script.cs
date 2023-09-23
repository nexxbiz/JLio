using TLio.Options;

namespace TLio.Models
{
    public class Script
    {
        public ScriptMetadata ScriptMetadata { get; set; } =new ScriptMetadata();
        public CommandsList Commands { get; set; } = new CommandsList();

        public string DataEcosystemName { get; set; } = ScriptOptions.DataEcosystemName;
        public bool StopOnError { get; set; }
    }
}