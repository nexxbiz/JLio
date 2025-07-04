﻿using System.Collections.Generic;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core;

public class NotFoundCommand : ICommand
{
    protected internal NotFoundCommand(string fullCommandText)
    {
        CommandName = fullCommandText;
    }

    public CommandRegistration CommandRegistration { get; set; }

    public string CommandName { get; }

    public JLioExecutionResult Execute(JToken data, IExecutionContext context)
    {
        context.LogError(CoreConstants.CommandExecution,
            $"script contains a unknown command : {CommandName}");
        return new JLioExecutionResult(false, data);
    }

    public ValidationResult ValidateCommandInstance()
    {
        return new ValidationResult
        {
            ValidationMessages = new List<string> {$"script contains a unknown command : {CommandName}"}
        };
    }
}