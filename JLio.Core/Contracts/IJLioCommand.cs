﻿using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IJLioCommand
    {
        string CommandName { get; }

        JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options);

        ValidationResult ValidateCommandInstance();
    }
}