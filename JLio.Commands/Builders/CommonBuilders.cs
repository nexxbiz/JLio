using JLio.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Commands.Builders
{
    public static class CommonBuilders
    {

        public static NewLine AddLine(this CommandLines source)
        {
            return new NewLine(source);
        }
    }
        public class NewLine
        {
            public NewLine(CommandLines script)
            {
                Script = script;
            }

            public CommandLines Script { get; }
        }
}
