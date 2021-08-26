using JLio.Core.Models;

namespace JLio.Commands.Builders
{
    public static class CopyBuilders
    {
        public static FromPathContainer Copy(this JLioScript source, string fromPath)
        {
            return new FromPathContainer(source, fromPath);
        }

        public static JLioScript To(this FromPathContainer source, string toPath)
        {
            source.Script.AddLine(new Copy(source.FromPath, toPath));
            return source.Script;
        }

        public class FromPathContainer
        {
            public FromPathContainer(JLioScript source, string fromPath)
            {
                Script = source;
                FromPath = fromPath;
            }

            internal string FromPath { get; }

            internal JLioScript Script { get; }
        }
    }
}