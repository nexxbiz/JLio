using JLio.Core.Models;


namespace JLio.Commands.Builders
{
    public static class MoveBuilders
    {
        public static FromPathMoveContainer Move(this JLioScript source, string fromPath)
        {
            return new FromPathMoveContainer(source, fromPath);
        }

        public static JLioScript To(this FromPathMoveContainer source, string toPath)
        {
            source.Script.AddLine(new Move(source.FromPath, toPath));
            return source.Script;
        }

        public class FromPathMoveContainer
        {
            public FromPathMoveContainer(JLioScript source, string fromPath)
            {
                Script = source;
                FromPath = fromPath;
            }

            internal JLioScript Script { get; }
            internal string FromPath { get; }
        }
    }
}
