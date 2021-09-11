using JLio.Core.Contracts;
using JLio.Core.Extensions;

namespace JLio.Core.Models
{
    public class ExecutionOptions : IExecutionOptions
    {
        public IItemsFetcher ItemsFetcher { get; set; }
        public IExecutionLogger Logger { get; set; }

        public static ExecutionOptions CreateDefault()
        {
            return new ExecutionOptions
                {ItemsFetcher = new JsonPathItemsFetcher(), Logger = new ExecutionLogger()};
        }
    }
}