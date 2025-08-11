namespace JLio.Extensions.Text.Builders;

public static class IsEmptyBuilders
{
    public static IsEmpty IsEmpty(string value)
    {
        return new IsEmpty(value);
    }
}