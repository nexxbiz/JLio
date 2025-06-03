using System.Collections.Generic;

namespace JLio.Core.Models.Path;

public class ChoppedElements : List<ChoppedElement>
{
    public new void Add(ChoppedElement element)
    {
        if (element != null) base.Add(element);
    }
}