using TLio.Options;

namespace TLio.Models
{
    public class Script<T>
    {
        public ScriptMetadata ScriptMetadata { get; set; } =new ScriptMetadata();
        public CommandsList<T> Commands { get; set; } = new CommandsList<T>();

        public string DataEcosystemName { get; set; } = ScriptOptions.DataEcosystemName;
        public bool StopOnError { get; set; }
    }
}