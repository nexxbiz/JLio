using JLio.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Commands.Builders
{
    public static class CommonBuilders
    {
        //todo: needs to be removed: override the normal add in jlioscript
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
