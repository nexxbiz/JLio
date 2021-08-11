using JLio.Core.Contracts;
using JLio.Core.Extentions;

namespace JLio.Core.Models
{
    public class JLioExecutionOptions : IJLioExecutionOptions
    {
        public IItemsFetcher ItemsFetcher { get; set; }
        public IJLioExecutionLogger Logger { get; set; }

        public static JLioExecutionOptions CreateDefault()
        {
            return new JLioExecutionOptions
            {
                Logger = null,
                ItemsFetcher = new JsonPathItemsFetcher()
            };
        }
    }
}