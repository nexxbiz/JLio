using System;
using System.Collections.Generic;
using System.Linq;
using JLio.Core.Models.Path;

//[assembly: publicsVisibleTo("JLio.UnitTests")]

namespace JLio.Core.Extentions
{
    //needs to become a part of a json converter. it should be injected into the script parser next to Command converter
    // the result of the parse should be a fully types version of the script ready to be executed

    public class FunctionAnalysis
    {
        public FunctionAnalysis(string text)
        {
            InitialText = text;
            FunctionName = GetFunctionName();
            if (IsFunction) Arguments = GetArguments();
        }

        public List<string> Arguments { get; } = new List<string>();

        public string FunctionName { get; }

        public string InitialText { get; }

        public bool IsFunction => InitialText.StartsWith(JLioConstants.FunctionStartCharacters);

        public static FunctionAnalysis DoAnalysis(string text)
        {
            return new FunctionAnalysis(text);
        }

        private List<string> GetArguments()
        {
            return SplitText.GetChoppedElements(
                InitialText.Substring(InitialText.IndexOf(
                                          JLioConstants.FunctionArgumentsStartCharacters,
                                          StringComparison.Ordinal)
                                      + JLioConstants.FunctionArgumentsStartCharacters.Length)
                    .TrimEnd(JLioConstants.FunctionArgumentsEndCharacters.ToCharArray()),
                JLioConstants.ArgumentsDelimeter,
                JLioConstants.ArgumentLevelPairs).Select(i => i.Text).ToList();
        }

        private string GetFunctionName()
        {
            if (!IsFunction) return string.Empty;
            var functionIndicatorLength = JLioConstants.FunctionStartCharacters.Length;
            return InitialText.Substring(functionIndicatorLength,
                    InitialText.IndexOf(JLioConstants.FunctionArgumentsStartCharacters,
                        StringComparison.InvariantCulture) - functionIndicatorLength)
                .Trim(' ');
        }
    }
}