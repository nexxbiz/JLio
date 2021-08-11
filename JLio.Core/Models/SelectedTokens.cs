using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class SelectedTokens : List<JToken>
    {
        public SelectedTokens(IEnumerable<JToken> tokens)
        {
            AddRange(tokens);
        }

        public bool AreSameTokenTypes
        {
            get { return this.Select(t => t.Type).Distinct().Count() < 2; }
        }

        public JTokenType GetTokenType
        {
            get
            {
                if (this.Any() && AreSameTokenTypes) return this.First().Type;
                return JTokenType.Undefined;
            }
        }

        public IEnumerable<T> GetTokensBySystemType<T>()
        {
            return this.OfType<T>();
        }

        public IEnumerable<JToken> GetTokens(JTokenType type)
        {
            return this.Where(i => i.Type == type).Select(i => i);
        }
    }
}