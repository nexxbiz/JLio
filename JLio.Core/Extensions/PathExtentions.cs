using System.Collections.Generic;
using System.Linq;
using JLio.Core.Models.Path;

namespace JLio.Core.Extensions
{
    public static class PathExtentions
    {
        public static string ToPathString(this IEnumerable<PathElement> source)
        {
            return source.ToList().ToPathString();
        }

        public static string ToPathString(this List<PathElement> source)
        {
            return string.Join(".", source.Select(i => i.PathElementFullText)).TrimEnd('.');
        }
    }
}