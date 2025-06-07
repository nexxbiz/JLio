# Calculate Function Documentation

## Overview

The `Calculate` function evaluates a mathematical expression and returns the numeric result. Parts of the expression can reference other values or functions by placing them inside square brackets (`[]`). Each referenced value must resolve to a single token.

## Syntax

### Function Expression Format
```json
"=calculate('2 + [$.a] * 3')"
```

### Programmatic Usage
```csharp
var calc = new Calculate("1 + [$.value]");
```

## Parameters

- **Expression**: A string containing the mathematical expression.
- **Placeholders**: References to other values or functions enclosed in `[]`.

## Example
```json
{
  "path": "$.result",
  "value": "=calculate('2 + [$.number]')",
  "command": "add"
}
```
