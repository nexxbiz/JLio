# Advanced Scenarios

JLio supports advanced scripting features such as nested functions and conditional execution.

## Nested Functions

Functions can be combined by using the result of one function as the argument of another. For example:

```json
{
  "path": "$.demo",
  "value": "=toString(parse($.raw))",
  "command": "add"
}
```

## Conditional Execution

Use the `ifElse` command to execute different scripts based on a condition. Each branch contains a nested script.

## Decision Tables

The `decisionTable` command allows mapping a value to a script. Depending on the input value a particular command set will be executed.

These capabilities enable complex transformation strategies.
