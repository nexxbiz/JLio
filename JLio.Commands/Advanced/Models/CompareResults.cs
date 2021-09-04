﻿using System.Collections.Generic;
using System.Linq;

namespace JLio.Commands.Advanced.Models
{
    public class CompareResults : List<CompareResult>
    {
        public CompareResults()
        {
        }

        public CompareResults(CompareResult result)
        {
            Add(result);
        }

        public bool ContainsIsDifferenceResult()
        {
            return this.Any(i => i.IsDifference);
        }
    }
}