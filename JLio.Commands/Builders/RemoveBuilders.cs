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
    public static class RemoveBuilders
    {
        public static JLioScript Remove(this NewLine source, string path)
        {
            source.Script.AddLine(new Remove(path));
            return source.Script;
        }

    }
}
