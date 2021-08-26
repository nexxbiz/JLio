using JLio.Core.Contracts;
using JLio.Core.Extensions;

namespace JLio.Core.Models
{
    public class JLioExecutionOptions : IExecutionOptions
    {
        public IItemsFetcher ItemsFetcher { get; set; }
        public IJLioExecutionLogger Logger { get; set; }

        public static JLioExecutionOptions CreateDefault()
        {
            return new JLioExecutionOptions
                {ItemsFetcher = new JsonPathItemsFetcher(), Logger = new JLioExecutionLogger()};
        }
    }
}