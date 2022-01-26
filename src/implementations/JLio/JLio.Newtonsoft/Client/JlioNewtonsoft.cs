using JLio.Newtonsoft.Mutators;
using Lio.Core.Options;

namespace JLio.Newtonsoft.Client;

public static class JlioNewtonsoft
{
    public static LioOptions Options()
    {
        return new LioOptions
        {
            Mutator = typeof(NewtonsoftMutator)
        };
    }
}