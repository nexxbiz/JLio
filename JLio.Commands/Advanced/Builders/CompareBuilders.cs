using JLio.Commands.Advanced.Settings;
using JLio.Core.Models;

namespace JLio.Commands.Advanced.Builders
{
    public static class CompareBuilders
    {
        public static CompareWithContainer With(this ComparePathContainer source, string secondPath)
        {
            return new CompareWithContainer(source, secondPath);
        }

        public static CompareSettingsContainer Using(this CompareWithContainer source, CompareSettings settings)
        {
            return new CompareSettingsContainer(source.Script, source.Path, source.SecondPath,
                settings);
        }

        public static CompareSettingsContainer UsingDefaultSettings(this CompareWithContainer source)
        {
            return source.Using(CompareSettings.CreateDefault());
        }

        public static JLioScript SetResultOn(this CompareSettingsContainer source, string resultPath)
        {
            source.Script.AddLine(new Compare
            {
                FirstPath = source.FirstPath,
                Settings = source.CompareSettings,
                ResultPath = resultPath,
                SecondPath = source.SecondPath
            });
            return source.Script;
        }

        public static ComparePathContainer Compare(this JLioScript source, string path)
        {
            return new ComparePathContainer(source, path);
        }

        public class ComparePathContainer
        {
            public ComparePathContainer(JLioScript source, string firstPath)
            {
                Script = source;
                FirstPath = firstPath;
            }

            internal string FirstPath { get; }

            internal JLioScript Script { get; }
        }

        public class CompareSettingsContainer
        {
            public CompareSettingsContainer(JLioScript source, string firstPath, string secondPath,
                CompareSettings settings)
            {
                Script = source;
                FirstPath = firstPath;
                SecondPath = secondPath;
                CompareSettings = settings;
            }

            internal CompareSettings CompareSettings { get; }

            internal string FirstPath { get; }

            internal JLioScript Script { get; }

            internal string SecondPath { get; }
        }

        public class CompareWithContainer
        {
            public CompareWithContainer(ComparePathContainer source, string secondPath)
            {
                Script = source.Script;
                Path = source.FirstPath;
                SecondPath = secondPath;
            }

            internal string Path { get; }
            internal JLioScript Script { get; }
            internal string SecondPath { get; }
        }
    }
}