namespace JLio.Extensions.Text.Builders;

public static class ConcatBuilders
{
    public static Concat Concat(params string[] arguments)
    {
        return new Concat(arguments);
    }
}
