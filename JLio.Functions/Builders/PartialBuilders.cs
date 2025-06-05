namespace JLio.Functions.Builders;

public static class PartialBuilders
{
    public static Partial Partial(params string[] arguments)
    {
        return new Partial(arguments);
    }
}
