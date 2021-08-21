using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static JLio.Commands.Builders.CommonBuilders;

namespace JLio.Commands.Builders
{
    public static class MoveBuilders
    {
        public static FromPathMoveContainer Move(this NewLine source, string fromPath)
        {
            return new FromPathMoveContainer(source.Script, fromPath);
        }

        public static JLioScript To(this FromPathMoveContainer source, string toPath)
        {
            source.Script.AddLine(new Move(source.FromPath, toPath));
            return source.Script;
        }

        public class FromPathMoveContainer
        {
            public FromPathMoveContainer(JLioScript source, string fromPath)
            {
                Script = source;
                FromPath = fromPath;
            }

            internal JLioScript Script { get; }
            internal string FromPath { get; }
        }
    }
}
