namespace JLio.Functions.Builders;

public static class IndirectBuilders
{
    public static Indirect Indirect(string pathToPath)
    {
        return new Indirect(pathToPath);
    }
}