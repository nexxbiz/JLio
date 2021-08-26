using System.Collections.Generic;

namespace JLio.Core.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationMessages { get; set; } = new List<string>();
    }
}