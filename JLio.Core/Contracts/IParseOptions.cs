using System.Collections.Generic;
using Newtonsoft.Json;

namespace JLio.Core.Contracts;

public interface IParseOptions
{
    JsonConverter JLioCommandConverter { get; set; }
    JsonConverter JLioFunctionConverter { get; set; }
}