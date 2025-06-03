using JLio.Commands.Advanced.Settings;
using JLio.Core.Models;

namespace JLio.Commands.Advanced.Builders;

public static class MergeBuilders
{
    public static MergeWithContainer With(this MergePathContainer source, string targetPath)
    {
        return new MergeWithContainer(source, targetPath);
    }

    public static JLioScript Using(this MergeWithContainer source, MergeSettings settings)
    {
        source.Script.AddLine(new Merge
            {Path = source.Path, TargetPath = source.TargetPath, Settings = settings});
        return source.Script;
    }

    public static JLioScript UsingDefaultSettings(this MergeWithContainer source)
    {
        source.Script.AddLine(new Merge
            {Path = source.Path, TargetPath = source.TargetPath, Settings = MergeSettings.CreateDefault()});
        return source.Script;
    }

    public static MergePathContainer Merge(this JLioScript source, string path)
    {
        return new MergePathContainer(source, path);
    }

    public class MergePathContainer
    {
        public MergePathContainer(JLioScript source, string path)
        {
            Script = source;
            Path = path;
        }

        internal string Path { get; }

        internal JLioScript Script { get; }
    }

    public class MergeWithContainer
    {
        public MergeWithContainer(MergePathContainer source, string targetPath)
        {
            Script = source.Script;
            Path = source.Path;
            TargetPath = targetPath;
        }

        internal string Path { get; }
        internal JLioScript Script { get; }
        internal string TargetPath { get; }
    }
}