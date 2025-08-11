# ceiling Function

Rounds numbers up to the nearest integer.

## Overview

The `ceiling` function rounds any numeric value up to the nearest integer. Unlike rounding functions that round to the nearest value, `ceiling` always rounds upward, ensuring the result is never less than the original value. This function accepts **single numeric values only** - for complex expressions, use the `calculate` function.

## Syntax

### Expression Format
```json
"=ceiling(value)"
```

### Builder Pattern
```csharp
CeilingBuilders.Ceiling(argument)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| value | number/string/JSONPath | Yes | A single numeric value, numeric string, or JSONPath expression that resolves to a number |

## Examples

### Basic Usage
```json
{
  "path": "$.roundedUp",
  "value": "=ceiling(4.1)",
  "command": "add"
}
```

**Result**: `5`

### String Number Conversion
```json
{
  "path": "$.packages",
  "value": "=ceiling('12.7')",
  "command": "add"
}
```

**Result**: `13`

### JSONPath Integration
```json
{
  "path": "$.roundedValue",
  "value": "=ceiling($.decimalValue)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "decimalValue": 7.714
}
```

**Result**: `8`

### Practical Applications

#### Rounding Calculated Values
For division operations, use `calculate` first:

```json
[
  {
    "path": "$.ratio",
    "value": "=calculate('{{$.totalItems}} / {{$.batchSize}}')",
    "command": "add"
  },
  {
    "path": "$.minimumBatches",
    "value": "=ceiling($.ratio)",
    "command": "add"
  }
]
```

**Input Data**:
```json
{
  "totalItems": 157,
  "batchSize": 25
}
```

**Result**: `$.ratio = 6.28`, `$.minimumBatches = 7`

#### Storage Requirements
```json
[
  {
    "path": "$.storageRatio",
    "value": "=calculate('{{$.dataSize}} / {{$.unitCapacity}}')",
    "command": "add"
  },
  {
    "path": "$.storageUnitsNeeded",
    "value": "=ceiling($.storageRatio)",
    "command": "add"
  }
]
```

## Builder Pattern Usage

### Simple Ceiling
```csharp
var script = new JLioScript()
    .Add(CeilingBuilders.Ceiling("$.price"))
    .OnPath("$.roundedPrice");
```

### With Calculated Values
```csharp
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("{{$.items}} / {{$.containerSize}}"))
    .OnPath("$.ratio")
    .Add(CeilingBuilders.Ceiling("$.ratio"))
    .OnPath("$.containersRequired");
```

## Data Type Support

### Numeric Types
- **Integer**: Returns the same value (e.g., `7` ? `7`)
- **Float/Double**: Rounds up to nearest integer (e.g., `7.1` ? `8`)
- **Decimal**: Converts and rounds up (e.g., `6.99` ? `7`)

### String Numbers
Automatically converts numeric strings using InvariantCulture:
```json
"=ceiling('5.2')"   // Result: 6
"=ceiling('-2.8')"  // Result: -2
"=ceiling('0.1')"   // Result: 1
```

### JSONPath Values
Resolves JSONPath to a single numeric value:
```json
"=ceiling($.value)"        // ? Single value
"=ceiling($.data[0])"      // ? Specific array element
```

### ? What Doesn't Work
```json
"=ceiling($.a / $.b)"            // ? Arithmetic not supported
"=ceiling($.total * 1.2)"        // ? Expressions not supported  
"=ceiling(sum($.values[*]))"     // ? Function nesting not supported
```

### ? Use Calculate Instead
```json
"=calculate('ceiling({{$.a}} / {{$.b}})')"     // ? Use calculate for expressions
```

### Null Values
Treats `null` as `0`:
```json
"=ceiling(null)"  // Result: 0
```

## Mathematical Behavior

### Positive Numbers
- `ceiling(3.1)` = `4`
- `ceiling(3.0)` = `3` (no change needed)
- `ceiling(3.9)` = `4`

### Negative Numbers
For negative numbers, "up" means toward zero:
- `ceiling(-3.1)` = `-3`
- `ceiling(-3.9)` = `-3`
- `ceiling(-4.0)` = `-4` (no change needed)

### Zero and Small Numbers
- `ceiling(0)` = `0`
- `ceiling(0.1)` = `1`
- `ceiling(-0.1)` = `0`

## Error Handling

### Argument Validation
```json
// Correct usage
"=ceiling(4.5)"     // ? Result: 5
"=ceiling($.value)" // ? JSONPath to number

// Errors
"=ceiling()"        // ? Missing required argument
"=ceiling(1, 2)"    // ? Too many arguments
"=ceiling('abc')"   // ? Non-numeric string
"=ceiling({})"      // ? Object not supported
"=ceiling([1, 2])"  // ? Array not supported
```

### Error Messages
- **Missing arguments**: "ceiling requires exactly one argument"
- **Invalid types**: "ceiling can only handle numeric values. Current type = [Type]"
- **Parse failures**: Non-numeric strings will result in parsing errors

## Use Cases

### Business Applications
1. **Resource Planning**: Calculate minimum resources needed
2. **Inventory Management**: Determine container or package requirements  
3. **Financial Planning**: Round up budget estimates for safety margins
4. **Capacity Planning**: Ensure adequate service capacity

### Technical Applications
1. **Memory Allocation**: Round up to minimum block sizes
2. **Database Partitioning**: Calculate partition counts
3. **Load Distribution**: Determine minimum server instances
4. **File System**: Calculate required disk blocks

### Manufacturing
1. **Material Requirements**: Calculate minimum material quantities
2. **Production Batches**: Determine batch counts for orders
3. **Quality Assurance**: Set minimum sample sizes
4. **Packaging**: Calculate container requirements

## Integration Examples

### With Calculate for Complex Operations
```json
[
  {
    "path": "$.analysis.avgDemand",
    "value": "=avg($.demands[*])",
    "command": "add"
  },
  {
    "path": "$.analysis.minRequired",
    "value": "=ceiling($.analysis.avgDemand)",
    "command": "add"
  },
  {
    "path": "$.analysis.safetyStockRatio", 
    "value": "=calculate('{{$.analysis.minRequired}} * 1.2')",
    "command": "add"
  },
  {
    "path": "$.analysis.safetyStock",
    "value": "=ceiling($.analysis.safetyStockRatio)",
    "command": "add"
  }
]
```

### Multi-Step Business Logic
```csharp
var script = new JLioScript()
    // Calculate ratio using calculate function
    .Add(CalculateBuilders.Calculate("{{$.orderQuantity}} / {{$.packageSize}}"))
    .OnPath("$.packageRatio")
    // Round up to whole packages
    .Add(CeilingBuilders.Ceiling("$.packageRatio"))
    .OnPath("$.packagesNeeded")
    // Apply safety margin
    .Add(CalculateBuilders.Calculate("{{$.packagesNeeded}} * {{$.safetyFactor}}"))
    .OnPath("$.safetyMarginRatio")
    // Round up final result
    .Add(CeilingBuilders.Ceiling("$.safetyMarginRatio"))
    .OnPath("$.totalPackages");
```

## Related Functions

- **[floor](floor.md)**: Rounds down to nearest integer
- **[round](round.md)**: Rounds to nearest integer or decimal places
- **[abs](abs.md)**: Returns absolute value
- **[calculate](calculate.md)**: For complex expressions with division, multiplication, etc.

## Best Practices

### When to Use Ceiling
1. **Resource Allocation**: When you need to ensure adequate resources
2. **Safety Margins**: When rounding down could cause shortfalls
3. **Discrete Quantities**: When dealing with indivisible units
4. **Minimum Requirements**: When you need guaranteed minimums

### Combining with Calculate
```csharp
// Good: Multi-step approach for complex calculations
var script = new JLioScript()
    .Add(CalculateBuilders.Calculate("{{$.volume}} / {{$.capacity}} * {{$.safetyFactor}}"))
    .OnPath("$.ratio")
    .Add(CeilingBuilders.Ceiling("$.ratio"))
    .OnPath("$.containers");
```

### Avoid Common Mistakes
```json
// ? This won't work - no arithmetic support
{
  "path": "$.result",
  "value": "=ceiling($.a / $.b)",
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
    "value": "=ceiling($.ratio)",
    "command": "add"
  }
]
```

## Advanced Usage Patterns

### Conditional Ceiling
```json
{
  "path": "$.result",
  "command": "ifElse",
  "condition": "$.needsRoundUp == true",
  "then": [
    {
      "path": "$.value",
      "value": "=ceiling($.rawValue)",
      "command": "set"
    }
  ],
  "else": [
    {
      "path": "$.value", 
      "value": "$.rawValue",
      "command": "set"
    }
  ]
}
```

### Pipeline Processing
```csharp
var script = new JLioScript()
    // Step 1: Sum values
    .Add(SumBuilders.Sum("$.items[*].weight"))
    .OnPath("$.totalWeight")
    // Step 2: Calculate capacity ratio
    .Add(CalculateBuilders.Calculate("{{$.totalWeight}} / {{$.truckCapacity}}"))
    .OnPath("$.capacityRatio")
    // Step 3: Round up to whole trucks
    .Add(CeilingBuilders.Ceiling("$.capacityRatio"))
    .OnPath("$.trucksNeeded");