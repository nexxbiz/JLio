using Microsoft.Extensions.DependencyInjection;

namespace Lio.Core.Options
{
    public class LioOptionsBuilder
    {
        public LioOptionsBuilder(IServiceCollection serviceCollection)
        {
            LioOptions = new LioOptions();
            Services = serviceCollection;
        }

        public LioOptions LioOptions { get; }

        public IServiceCollection Services { get; }
    }
}