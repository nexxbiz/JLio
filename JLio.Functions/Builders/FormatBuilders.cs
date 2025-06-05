using JLio.Functions;

namespace JLio.Functions.Builders;

public static class FormatBuilders
{
    public static Format Format(string formatString)
    {
        return new Format(formatString);
    }

    public static FormatPathContainer FormatPath(string path)
    {
        return new FormatPathContainer(path);
    }

    public static Format UsingFormat(this FormatPathContainer source, string formatString)
    {
        return new Format(source.Path, formatString);
    }

    public class FormatPathContainer
    {
        public FormatPathContainer(string path)
        {
            Path = path;
        }

        internal string Path { get; }
    }
}
