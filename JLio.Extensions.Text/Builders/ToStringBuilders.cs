namespace JLio.Extensions.Text.Builders;

public static class ToStringBuilders
{
    public static new ToString ToString() 
    {
        return new ToString();
    }

    public static ToString ToString(string path)
    {
        return new ToString(path);
    }
}
