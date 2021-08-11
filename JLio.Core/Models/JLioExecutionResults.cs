using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class JLioExecutionResults : List<JLioExecutionResult>
    {
        public JToken Data { get; private set; }

        public bool Success
        {
            get { return this.All(i => i.Success); }
        }

        public new void Add(JLioExecutionResult result)
        {
            base.Add(result);
            Data = result.Data;
        }
    }
}