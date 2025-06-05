using JLio.Functions;

namespace JLio.Functions.Builders;

public static class PromoteBuilders
{
    public static Promote Promote(string newPropertyName)
    {
        return new Promote(newPropertyName);
    }

    public static Promote Promote(string path, string newPropertyName)
    {
        return new Promote(path, newPropertyName);
    }
}
