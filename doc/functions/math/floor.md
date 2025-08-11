# floor Function

Rounds numbers down to the nearest integer.

## Overview

The `floor` function rounds any numeric value down to the nearest integer. Unlike ceiling which always rounds up, floor always rounds downward, ensuring the result is never greater than the original value. This function accepts **single numeric values only** - for complex expressions, use the `calculate` function.

## Syntax

### Expression Format
```json
"=floor(value)"
```

### Builder Pattern
```csharp
FloorBuilders.Floor(argument)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| value | number/string/JSONPath | Yes | A single numeric value, numeric string, or JSONPath expression that resolves to a number |

## Examples

### Basic Usage
```json
{
  "path": "$.roundedDown",
  "value": "=floor(4.9)",
  "command": "add"
}
```

**Result**: `4`

### String Number Conversion
```json
{
  "path": "$.wholeUnits",
  "value": "=floor('7.8')",
  "command": "add"
}
```

**Result**: `7`

### JSONPath Integration
```json
{
  "path": "$.completeUnits",
  "value": "=floor($.decimalQuantity)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "decimalQuantity": 6.28
}
```

**Result**: `6`

### Practical Applications

#### Complete Units from Division
For division operations, use `calculate` first:

```json
[
  {
    "path": "$.ratio",
    "value": "=calculate('{{$.totalQuantity}} / {{$.unitsPerPackage}}')",
    "command": "add"
  },
  {
    "path": "$.completePackages",
    "value": "=floor($.ratio)",
    "command": "add"
  }
]
```

**Input Data**:
```json
{
  "totalQuantity": 157,
  "unitsPerPackage": 25
}
```

**Result**: `$.ratio = 6.28`, `$.completePackages = 6`

#### Inventory Management
```json
[
  {
    "path": "$.boxRatio",
    "value": "=calculate('{{$.totalItems}} / {{$.itemsPerBox}}')",
    "command": "add"
  },
  {
    "path": "$.completeBoxes",
    "value": "=floor($.boxRatio)",
    "command": "add"
  }
]
```

#### Time Calculations
```json
[
  {
    "path": "$.hourRatio",
    "value": "=calculate('{{$.totalMinutes}} / 60')",
    "command": "add"
  },
  {
    "path": "$.completeHours",
    "value": "=floor($.hourRatio)",
    "command": "add"
  }
]
```

## Builder Pattern Usage

### Simple Floor Operation
```csharp
var script = new JLioScript()
    .Add(FloorBuilders.Floor("$.price"))
    .OnPath("$.wholeDollars");
```

### Complex Calculations
```csharp
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("{{$.budget}} / {{$.costPerUnit}}"))
    .OnPath("$.unitRatio")
    .Add(FloorBuilders.Floor("$.unitRatio"))
    .OnPath("$.maxUnits");
```

## Data Type Support

### Numeric Types
- **Integer**: Returns the same value (e.g., `5` ? `5`)
- **Positive Float**: Rounds down (e.g., `5.9` ? `5`)
- **Negative Float**: Rounds down (more negative) (e.g., `-2.1` ? `-3`)
- **Zero**: Unchanged (e.g., `0.0` ? `0`)

### String Numbers
Automatically converts numeric strings using InvariantCulture:
```json
"=floor('5.9')"   // Result: 5
"=floor('-2.1')"  // Result: -3
"=floor('0.9')"   // Result: 0
```

### JSONPath Values
Resolves JSONPath to a single numeric value:
```json
"=floor($.value)"        // ? Single value
"=floor($.data[0])"      // ? Specific array element
```

### ? What Doesn't Work
```json
"=floor($.a / $.b)"            // ? Arithmetic not supported
"=floor($.total * 0.8)"        // ? Expressions not supported  
"=floor(sum($.values[*]))"     // ? Function nesting not supported
```

### ? Use Calculate Instead
```json
"=calculate('floor({{$.a}} / {{$.b}})')"     // ? Use calculate for expressions
```

### Null Values
Treats `null` as `0`:
```json
"=floor(null)"  // Result: 0
```

## Mathematical Behavior

### Positive Numbers
Floor removes the fractional part:
- `floor(3.1)` = `3`
- `floor(3.9)` = `3`
- `floor(3.0)` = `3` (no change needed)

### Negative Numbers
For negative numbers, floor rounds toward negative infinity:
- `floor(-3.1)` = `-4`
- `floor(-3.9)` = `-4`  
- `floor(-3.0)` = `-3` (no change needed)

### Zero and Small Numbers
- `floor(0)` = `0`
- `floor(0.9)` = `0`
- `floor(-0.1)` = `-1`

## Error Handling

### Argument Validation
```json
// Correct usage
"=floor(4.7)"     // ? Result: 4
"=floor($.value)" // ? JSONPath to number

// Errors
"=floor()"        // ? Missing required argument
"=floor(1, 2)"    // ? Too many arguments
"=floor('abc')"   // ? Non-numeric string
"=floor({})"      // ? Object not supported
"=floor([1, 2])"  // ? Array not supported
```

### Error Messages
- **Missing arguments**: "floor requires exactly one argument"
- **Invalid types**: "floor can only handle numeric values. Current type = [Type]"
- **Parse failures**: Non-numeric strings will result in parsing errors

## Use Cases

### Business Applications
1. **Inventory Management**: Calculate complete units available
2. **Financial Planning**: Determine whole dollar amounts
3. **Resource Allocation**: Calculate maximum complete allocations
4. **Pricing Strategies**: Set price floors

### Technical Applications
1. **Array Indexing**: Ensure valid array indices
2. **Memory Management**: Calculate complete memory blocks
3. **Batch Processing**: Determine complete batch counts
4. **Performance Metrics**: Calculate completed operations

### Manufacturing
1. **Production Planning**: Calculate complete production runs
2. **Material Usage**: Determine complete material units consumed
3. **Quality Control**: Calculate complete sample sets
4. **Capacity Planning**: Determine maximum complete capacity utilization

## Integration Examples

### With Calculate for Complex Operations
```json
[
  {
    "path": "$.analysis.totalQuantity",
    "value": "=sum($.quantities[*])",
    "command": "add"
  },
  {
    "path": "$.analysis.unitRatio",
    "value": "=calculate('{{$.analysis.totalQuantity}} / {{$.unitSize}}')",
    "command": "add"
  },
  {
    "path": "$.analysis.completeUnits",
    "value": "=floor($.analysis.unitRatio)",
    "command": "add"
  },
  {
    "path": "$.analysis.remainder", 
    "value": "=calculate('{{$.analysis.totalQuantity}} % {{$.unitSize}}')",
    "command": "add"
  }
]
```

### Complex Business Logic
```csharp
var script = new JLioScript()
    // Calculate maximum complete orders
    .Add(CalculateBuilders.Calculate("{{$.inventory}} / {{$.orderSize}}"))
    .OnPath("$.orderRatio")
    .Add(FloorBuilders.Floor("$.orderRatio"))
    .OnPath("$.maxOrders")
    // Calculate remaining inventory
    .Add(CalculateBuilders.Calculate("{{$.inventory}} % {{$.orderSize}}"))
    .OnPath("$.remainingInventory");
```

## Related Functions

- **[ceiling](ceiling.md)**: Rounds up to nearest integer
- **[round](round.md)**: Rounds to nearest integer or decimal places
- **[abs](abs.md)**: Returns absolute value
- **[calculate](calculate.md)**: For complex expressions with division, multiplication, etc.

## Best Practices

### When to Use Floor
1. **Resource Constraints**: When you can't exceed available resources
2. **Complete Units**: When dealing with indivisible quantities
3. **Index Calculations**: For array or collection indexing
4. **Financial Floors**: When setting minimum price thresholds

### Combining with Calculate
```csharp
// Calculate capacity utilization
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("{{$.demand}} / {{$.capacity}} * 100"))
    .OnPath("$.utilizationRatio")
    .Add(FloorBuilders.Floor("$.utilizationRatio"))
    .OnPath("$.utilizationPercent");
```

### Avoid Common Mistakes
```json
// ? This won't work - no arithmetic support
{
  "path": "$.result",
  "value": "=floor($.a / $.b)",
  "command": "add"
}

// ? Use this pattern instead  
[
  {
    "path": "$.ratio",
    "value": "=calculate('{{$.a}} / {{$.b}}')",
    "command": "add"
  },
  {
    "path": "$.result",
    "value": "=floor($.ratio)",
    "command": "add"
  }
]
```

## Advanced Usage Patterns

### Conditional Floor Operations
```json
{
  "path": "$.processedValue",
  "command": "ifElse",
  "condition": "$.allowPartialUnits == false",
  "then": [
    {
      "path": "$.result",
      "value": "=floor($.rawValue)",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result",
      "value": "$.rawValue", 
      "command": "add"
    }
  ]
}
```

### Multi-Step Resource Calculations
```csharp
var script = new JLioScript()
    // Calculate base allocation
    .Add(CalculateBuilders.Calculate("{{$.totalBudget}} / {{$.departments}}"))
    .OnPath("$.allocationRatio")
    .Add(FloorBuilders.Floor("$.allocationRatio"))
    .OnPath("$.baseAllocation")
    // Calculate remaining budget
    .Add(CalculateBuilders.Calculate("{{$.totalBudget}} - ({{$.baseAllocation}} * {{$.departments}})"))
    .OnPath("$.remainingBudget");
```

### Inventory Optimization
```json
[
  {
    "path": "$.boxRatio",
    "value": "=calculate('{{$.totalItems}} / {{$.itemsPerBox}}')",
    "command": "add"
  },
  {
    "path": "$.inventoryAnalysis.completeBoxes",
    "value": "=floor($.boxRatio)",
    "command": "add"
  },
  {
    "path": "$.inventoryAnalysis.looseItems",
    "value": "=calculate('{{$.totalItems}} % {{$.itemsPerBox}}')",
    "command": "add"
  },
  {
    "path": "$.ceilingRatio",
    "value": "=calculate('{{$.totalItems}} / {{$.itemsPerBox}}')",
    "command": "add"
  },
  {
    "path": "$.inventoryAnalysis.totalBoxesNeeded",
    "value": "=ceiling($.ceilingRatio)",
    "command": "add"
  }
]
```

## Comparison with Related Functions

### Floor vs Ceiling vs Round
```json
// For value 4.7:
"=floor(4.7)"    // Result: 4 (always down)
"=ceiling(4.7)"  // Result: 5 (always up)  
"=round(4.7)"    // Result: 5 (nearest)

// For value 4.3:
"=floor(4.3)"    // Result: 4 (always down)
"=ceiling(4.3)"  // Result: 5 (always up)
"=round(4.3)"    // Result: 4 (nearest)

// For negative values (-2.3):
"=floor(-2.3)"   // Result: -3 (more negative)
"=ceiling(-2.3)" // Result: -2 (less negative)
"=round(-2.3)"   // Result: -2 (nearest)
```

### Practical Decision Guide
- **Use floor()** when you need to ensure you don't exceed limits
- **Use ceiling()** when you need to ensure adequate resources
- **Use round()** when you want the most accurate representation
- **Use calculate()** when you need arithmetic expressions