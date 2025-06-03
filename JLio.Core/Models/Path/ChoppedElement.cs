namespace JLio.Core.Models.Path;

public class ChoppedElement
{
    public ChoppedElement(string text)
    {
        Text = text.Trim(' ');
    }

    public string Text { get; }
}