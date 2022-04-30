using System.Collections.Generic;
using TLio.Contracts;
using TLio.Options;

namespace TLio.Models
{
    public class Script
    {
        public ScriptMetadata ScriptMetadata { get; set; } =new ScriptMetadata();
        public CommandsList Commands { get; set; }

        public string MutatorName { get; set; } = ScriptOptions.DefaultMutatorName;
        public string FetcherName { get; set; } = ScriptOptions.DefaultFetcherName;
    }
}