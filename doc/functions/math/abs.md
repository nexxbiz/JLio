# abs Function

Calculate the absolute value of a number.

## Overview

The `abs` function returns the absolute value (non-negative magnitude) of a numeric value. It removes the sign from negative numbers while leaving positive numbers unchanged. This function is essential for distance calculations, error measurements, and ensuring non-negative results.

## Syntax

### Expression Format
```json
"=abs(value)"
```

### Builder Pattern
```csharp
AbsBuilders.Abs(argument)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| value | number/string | Yes | The numeric value to get the absolute value of. Accepts numbers, numeric strings, or JSONPath expressions |

## Examples

### Basic Usage
```json
{
  "path": "$.absoluteValue",
  "value": "=abs(-5)",
  "command": "add"
}
```

**Result**: `5`

### String Number Conversion
```json
{
  "path": "$.magnitude",
  "value": "=abs('-42.7')",
  "command": "add"
}
```

**Result**: `42.7`

### JSONPath Integration
```json
{
  "path": "$.errorMagnitude",
  "value": "=abs($.actual - $.expected)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "actual": 95.2,
  "expected": 100
}
```

**Result**: `4.8`

### Practical Applications

#### Distance Calculation
```json
{
  "path": "$.distance",
  "value": "=abs($.pointA.x - $.pointB.x)",
  "command": "add"
}
```

#### Error Analysis
```json
{
  "path": "$.deviation",
  "value": "=abs($.measurement - $.target)",
  "command": "add"
}
```

#### Temperature Difference
```json
{
  "path": "$.temperatureDelta",
  "value": "=abs($.currentTemp - $.setPoint)",
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Absolute Value
```csharp
var script = new JLioScript()
    .Add(AbsBuilders.Abs("$.balance"))
    .OnPath("$.absBalance");
```

### Complex Calculations
```csharp
var script = new JLioScript()
    .Add(AbsBuilders.Abs("$.revenue - $.expenses"))
    .OnPath("$.netDifference")
    .Add(AbsBuilders.Abs("$.target - $.actual"))
    .OnPath("$.variance");
```

## Data Type Support

### Numeric Types
- **Positive Integer**: Unchanged (e.g., `7` ? `7`)
- **Negative Integer**: Sign removed (e.g., `-7` ? `7`)
- **Positive Float**: Unchanged (e.g., `3.14` ? `3.14`)
- **Negative Float**: Sign removed (e.g., `-3.14` ? `3.14`)
- **Zero**: Unchanged (e.g., `0` ? `0`)

### String Numbers
Automatically converts numeric strings using InvariantCulture:
```json
"=abs('5.2')"    // Result: 5.2
"=abs('-5.2')"   // Result: 5.2
"=abs('-0')"     // Result: 0
"=abs('0.0')"    // Result: 0
```

### Null Values
Treats `null` as `0`:
```json
"=abs(null)"  // Result: 0
```

## Mathematical Behavior

### Positive Numbers
All positive numbers remain unchanged:
- `abs(5)` = `5`
- `abs(3.14)` = `3.14`
- `abs(0.001)` = `0.001`

### Negative Numbers
All negative numbers become positive:
- `abs(-5)` = `5`
- `abs(-3.14)` = `3.14`
- `abs(-0.001)` = `0.001`

### Zero
Zero remains zero:
- `abs(0)` = `0`
- `abs(-0)` = `0`
- `abs(0.0)` = `0`

## Error Handling

### Argument Validation
```json
// Correct usage
"=abs(-4.5)"  // ? Result: 4.5

// Errors
"=abs()"         // ? Missing required argument
"=abs(1, 2)"     // ? Too many arguments
"=abs('text')"   // ? Non-numeric string
"=abs({})"       // ? Object not supported
"=abs([1, 2])"   // ? Array not supported
```

### Error Messages
- **Missing arguments**: "abs requires exactly one argument"
- **Invalid types**: "abs can only handle numeric values. Current type = [Type]"
- **Parse failures**: Non-numeric strings result in parsing errors

## Use Cases

### Financial Applications
1. **Account Balances**: Show magnitude regardless of positive/negative
2. **Variance Analysis**: Calculate absolute differences from targets
3. **Risk Assessment**: Measure deviation magnitude
4. **P&L Analysis**: Show absolute changes

### Scientific Applications
1. **Error Measurement**: Calculate measurement errors
2. **Distance Calculations**: Find distances between points
3. **Signal Processing**: Remove signal polarity
4. **Data Normalization**: Ensure positive values

### Quality Control
1. **Tolerance Checking**: Measure deviation from specifications
2. **Performance Metrics**: Calculate absolute performance differences
3. **Calibration**: Measure calibration errors
4. **Statistical Analysis**: Remove directional bias

## Performance Considerations

- **Execution Speed**: Extremely fast single operation
- **Memory Usage**: Minimal memory footprint
- **String Parsing**: Optimized with InvariantCulture
- **Numeric Precision**: Maintains full numeric precision

## Integration Examples

### With Statistical Functions
```json
[
  {
    "path": "$.deviations[*]",
    "value": "=abs(@.value - $.mean)",
    "command": "set"
  },
  {
    "path": "$.meanAbsoluteDeviation",
    "value": "=avg($.deviations[*])",
    "command": "add"
  }
]
```

### Complex Business Logic
```csharp
var script = new JLioScript()
    // Calculate budget variance
    .Add(AbsBuilders.Abs("$.actual - $.budget"))
    .OnPath("$.variance")
    // Determine if significant
    .Add(CalculateBuilders.Calculate("$.variance > $.threshold"))
    .OnPath("$.isSignificant");
```

### Data Validation
```json
{
  "path": "$.isWithinTolerance",
  "value": "=abs($.measured - $.target) <= $.tolerance",
  "command": "add"
}
```

## Related Functions

- **[min](min.md)**: Find minimum value (can work with negative numbers)
- **[max](max.md)**: Find maximum value (can work with negative numbers)
- **[sqrt](sqrt.md)**: Calculate square root (requires non-negative input)
- **[pow](pow.md)**: Calculate power (abs useful for ensuring positive bases)
- **[subtract](subtract.md)**: Subtraction (abs useful for distances)

## Mathematical Properties

### Identity Properties
- `abs(x) ? 0` for all x
- `abs(x) = abs(-x)` (symmetry)
- `abs(0) = 0`
- `abs(x) = x` when x ? 0

### Useful Relationships
- `abs(a - b)` gives distance between a and b
- `abs(x)² = x²` (square of absolute equals square of original)
- `max(x, -x) = abs(x)` (alternative definition)

## Best Practices

### When to Use Abs
1. **Distance/Difference Calculations**: Always use for measuring distances
2. **Error Metrics**: Essential for unbiased error measurements
3. **Data Cleaning**: Remove sign inconsistencies
4. **Magnitude Comparisons**: When direction doesn't matter

### Common Patterns
```csharp
// Distance between two points
var script = new JLioScript()
    .Add(AbsBuilders.Abs("$.pointA - $.pointB"))
    .OnPath("$.distance");

// Error rate calculation
var script = new JLioScript()
    .Add(AbsBuilders.Abs("($.actual - $.expected) / $.expected"))
    .OnPath("$.errorRate");
```

### Avoid When
1. **Directional Information Needed**: Don't use when sign matters
2. **Already Positive Values**: Unnecessary overhead
3. **Complex Number Operations**: Not applicable (JLio handles real numbers only)

## Advanced Usage Patterns

### Conditional Processing
```json
{
  "path": "$.status",
  "command": "ifElse", 
  "condition": "=abs($.variance) > $.threshold",
  "then": [
    {
      "path": "$.alert",
      "value": "HIGH_VARIANCE",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.alert",
      "value": "NORMAL",
      "command": "add"
    }
  ]
}
```

### Multi-Dimensional Calculations
```csharp
// Calculate Euclidean distance components
var script = new JLioScript()
    .Add(AbsBuilders.Abs("$.x1 - $.x2"))
    .OnPath("$.deltaX")
    .Add(AbsBuilders.Abs("$.y1 - $.y2"))
    .OnPath("$.deltaY")
    .Add(SqrtBuilders.Sqrt("pow($.deltaX, 2) + pow($.deltaY, 2)"))
    .OnPath("$.distance");
```

### Quality Control Metrics
```json
{
  "path": "$.qualityMetrics",
  "value": {
    "averageError": "=avg(abs($.measurements[*] - $.target))",
    "maxError": "=max(abs($.measurements[*] - $.target))",
    "errorRate": "=count($.measurements[*][abs(@ - $.target) > $.tolerance]) / count($.measurements[*])"
  },
  "command": "add"
}
```