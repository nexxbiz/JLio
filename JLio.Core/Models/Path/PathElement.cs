using System.Linq;

namespace JLio.Core.Models.Path
{
    public class PathElement
    {
        public PathElement(string pathElementFullText)
        {
            PathElementFullText = pathElementFullText;
        }

        public string ArrayNotation => !string.IsNullOrEmpty(PathElementFullText)
            ? PathElementFullText.Contains('[') ? PathElementFullText.Substring(PathElementFullText.IndexOf('[')) :
            string.Empty
            : string.Empty;

        public string ArrayNotationInnerText => ArrayNotation.TrimStart('[').TrimEnd(']');

        public string ElementName => !string.IsNullOrEmpty(PathElementFullText)
            ? PathElementFullText.Contains('[') ? PathElementFullText.Substring(0, PathElementFullText.IndexOf('[')) :
            PathElementFullText
            : PathElementFullText;

        public bool HasArrayIndicator => PathElementFullText.Contains('[') && PathElementFullText.EndsWith("]");
        public string PathElementFullText { get; }
        public bool RecursiveDescentIndicator => PathElementFullText == string.Empty;
    }
}