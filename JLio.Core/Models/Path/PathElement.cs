namespace JLio.Core.Models.Path;

public class PathElement
{
    public PathElement(string pathElementFullText)
    {
        PathElementFullText = pathElementFullText;
    }

    public string ArrayNotation => GetArrayNotation();

    public string ArrayNotationInnerText => ArrayNotation.TrimStart('[').TrimEnd(']');

    public string ElementName => !string.IsNullOrEmpty(PathElementFullText)
        ? PathElementFullText.Contains('[') ? PathElementFullText.Substring(0, PathElementFullText.IndexOf('[')) :
        PathElementFullText
        : PathElementFullText;

    public bool HasArrayIndicator => GetHasArrayIndicator();

    public string PathElementFullText { get; }
    public bool RecursiveDescentIndicator => PathElementFullText == string.Empty;

    private string GetArrayNotation()
    {
        if (!HasArrayIndicator) return string.Empty;
        var cleanedElement = GetCleanedElement();
        var start = cleanedElement.Substring(cleanedElement.IndexOf('['));
        return start.Substring(0, start.IndexOf(']') + 1);
    }

    private bool GetHasArrayIndicator()
    {
        var cleanedElement = GetCleanedElement();

        return cleanedElement.Contains('[') && cleanedElement.EndsWith("]");
    }

    private string GetCleanedElement()
    {
        return PathElementFullText.Replace("['", "").Replace("']", "");
    }
}