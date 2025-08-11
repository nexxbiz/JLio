namespace JLio.Extensions.Text.Builders;

public static class LengthBuilders
{
    public static Length Length(string text)
    {
        return new Length(text);
    }
}