using JLio.Core.Models;

namespace JLio.Commands.Builders
{
    public static class RemoveBuilders
    {
        public static JLioScript Remove(this JLioScript source, string path)
        {
            source.AddLine(new Remove(path));
            return source;
        }
    }
}