using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Core.Models
{
    public class ValidationResult
    {
        public List<string> ValidationMessages { get; set; } = new List<string>();

        public bool IsValid { get; set; }
    }
}
