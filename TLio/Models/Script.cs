using TLio.Options;

namespace TLio.Models
{
    public class Script
    {
        public ScriptMetadata ScriptMetadata { get; set; } =new ScriptMetadata();
        public CommandsList Commands { get; set; } = new CommandsList();

        public string MutatorName { get; set; } = ScriptOptions.DefaultMutatorName;
        public string FetcherName { get; set; } = ScriptOptions.DefaultFetcherName;
        public bool StopOnError { get; set; }
    }
}