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
    public static class CopyBuilders
    {
        public static FromPathContainer Copy(this NewLine source, string fromPath)
        {
            return new FromPathContainer(source.Script, fromPath);
        }

        public static JLioScript To(this FromPathContainer source, string toPath)
        {
            source.Script.AddLine(new Copy(source.FromPath, toPath));
            return source.Script;
        }

        public class FromPathContainer
        {
            public FromPathContainer(JLioScript source, string fromPath)
            {
                Script = source;
                FromPath = fromPath;
            }

            internal JLioScript Script { get; }
            internal string FromPath { get; }
        }
    }
}
