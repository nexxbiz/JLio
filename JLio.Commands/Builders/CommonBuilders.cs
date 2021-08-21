using JLio.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Commands.Builders
{
    public static class CommonBuilders
    {

        public static NewLine AddScriptLine(this JLioScript source)
        {
            return new NewLine(source);
        }
    }
        public class NewLine
        {
            public NewLine(JLioScript script)
            {
                Script = script;
            }

            public JLioScript Script { get; }
        }
}
