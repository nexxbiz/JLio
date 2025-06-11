using System.Collections.Generic;
using JLio.Core.Models;

namespace JLio.Commands.Advanced.Builders;

public static class DistinctBuilders
{
    public static JLioScript Distinct(this JLioScript source, string path)
    {
        source.AddLine(new Distinct(path));
        return source;
    }

    public static JLioScript Distinct(this JLioScript source, string path, List<string> keyPaths)
    {
        source.AddLine(new Distinct(path) { KeyPaths = keyPaths });
        return source;
    }
}

