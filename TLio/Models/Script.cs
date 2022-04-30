using System.Collections.Generic;
using TLio.Contracts;

namespace TLio.Models
{
    public class Script
    {
        public ScriptMetadata ScriptMetadata { get; set; }
        public IList<ICommand> Commands { get; set; }
        
        public string MutatorType { get; set; }
        public string FetcherType { get; set; }
    }
}