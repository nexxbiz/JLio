using System.Collections.Generic;
using System.Linq;
using JLio.Core.Models.Path;

namespace JLio.Core.Extensions
{
    public class JsonSplittedPath
    {
        private const char pathDelimiter = '.';
        public readonly List<PathElement> Elements = new List<PathElement>();

        public JsonSplittedPath(string path)
        {
            var element = string.Empty;
            var arrayNotationLevel = 0;
            foreach (var c in path)
            {
                if (c == pathDelimiter && arrayNotationLevel == 0)
                {
                    Elements.Add(new PathElement(element));
                    element = string.Empty;
                }
                else
                {
                    element = $"{element}{c}";
                }

                if (c == '[') arrayNotationLevel++;
                if (c == ']') arrayNotationLevel--;
            }

            if (element.Any()) Elements.Add(new PathElement(element));
        }

        public IEnumerable<PathElement> ConstructionPath => Elements.Skip(GetSelectionPathIndex() + 1);

        public bool IsSearchingForObjectsByName =>
            Elements.Count > 1 && Elements[Elements.Count - 2].RecursiveDescentIndicator;

        public PathElement LastElement => Elements.Last();
        public string LastName => LastElement.PathElementFullText;

        public IEnumerable<PathElement> ParentElements =>
            Elements.Any()
                ? Elements.Take(Elements.Count - 1)
                : new List<PathElement>();

        public IEnumerable<PathElement> SelectionPath => Elements.Take(GetSelectionPathIndex() + 1);

        private int GetSelectionPathIndex()
        {
            for (var i = Elements.Count - 1; i > 0; i--)
            {
                if (Elements[i].RecursiveDescentIndicator) return i + 1;

                if (Elements[i].HasArrayIndicator) return i;
            }

            return 0;
        }
    }
}