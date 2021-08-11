using JLio.Core.Contracts;

namespace JLio.Client
{
    public class JLioExecutionOptions : IJLioExecutionOptions
    {
        public IItemsFetcher ItemsFetcher { get; set; }
        public IJLioExecutionLogger Logger { get; set; }

        public static JLioExecutionOptions CreateDefault()
        {
            return new JLioExecutionOptions {Logger = null};
        }
    }
}