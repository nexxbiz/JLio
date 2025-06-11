# Subtract Function Documentation

## Overview

The `Subtract` function subtracts the second numeric argument from the first. Each argument may resolve to a single value or a collection of values. When a JSONPath expression or array returns multiple tokens, those tokens are summed before performing the subtraction. Null values are ignored and non‑numeric values cause the function to fail.

## Installation

### Extension Pack Registration
```csharp
// Register math functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterMath();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Syntax

### Function Expression Format
```json
"=subtract(base, value)"
```

Examples:
```json
"=subtract(10, 3)"                 // 7
"=subtract($.a, $.b)"               // subtract tokens
"=subtract($.values[*], 5)"         // first argument is array
"=subtract($.a[*], $.b[*])"         // both arguments are arrays
```

### Programmatic Usage
```csharp
// With literal arguments
var subtractFunction = new Subtract("10", "3");

// Empty constructor for dynamic arguments
var subtractFunction = new Subtract();
```

### Builder Pattern
```csharp
var subtractFunction = SubtractBuilders.Subtract("10", "3");
var pathSubtract = SubtractBuilders.Subtract("$.a[*]", "$.b[*]");
```

## Parameters

- **base**: first numeric value or collection
- **value**: second numeric value or collection
- **Argument Types**:
  - Literal numbers or numeric strings
  - JSONPath expressions
  - Array references (including nested arrays)
- **Return Type**: Double (JValue)

## Examples

### Basic Subtraction
```json
{
  "path": "$.difference",
  "value": "=subtract($.a, $.b)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "a": 10,
  "b": 3
}
```

**Result**:
```json
{
  "a": 10,
  "b": 3,
  "difference": 7
}
```

### Array Subtraction
```json
{
  "path": "$.diff",
  "value": "=subtract($.list[*], 2)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "list": [2, 3, 4]
}
```

**Result**:
```json
{
  "list": [2, 3, 4],
  "diff": 7
}
```
