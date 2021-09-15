using System.Collections.Generic;
using System.Linq;

namespace JLio.Core.Models
{
    public class ValidationResult
    {
        public bool IsValid => !ValidationMessages.Any();
        public List<string> ValidationMessages { get; set; } = new List<string>();
    }
}