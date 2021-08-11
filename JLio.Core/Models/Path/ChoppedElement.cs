namespace JLio.Core.Models.Path
{
    internal class ChoppedElement
    {
        internal ChoppedElement(string text)
        {
            Text = text.Trim(' ');
        }

        internal string Text { get; }
    }
}