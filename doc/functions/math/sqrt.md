# sqrt Function

Calculate the square root of a number.

## Overview

The `sqrt` function calculates the square root of a numeric value, supporting numbers, numeric strings, and JSONPath expressions. It's essential for statistical calculations, geometric computations, and mathematical analysis requiring root operations.

## Syntax

### Expression Format
```json
"=sqrt(value)"
```

### Builder Pattern
```csharp
SqrtBuilders.Sqrt(argument)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| value | number/string | Yes | The numeric value to find the square root of (must be non-negative) |

## Examples

### Basic Square Root
```json
{
  "path": "$.result",
  "value": "=sqrt(9)",
  "command": "add"
}
```

**Result**: `3`

### Decimal Values
```json
{
  "path": "$.distance",
  "value": "=sqrt(25.0)",
  "command": "add"
}
```

**Result**: `5.0`

### String Number Support
```json
{
  "path": "$.calculation",
  "value": "=sqrt('16.0')",
  "command": "add"
}
```

**Result**: `4.0`

### Pythagorean Theorem
```json
{
  "path": "$.hypotenuse",
  "value": "=sqrt(pow($.sideA, 2) + pow($.sideB, 2))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "sideA": 3,
  "sideB": 4
}
```

**Result**: `5.0`

### Standard Deviation Calculation
```json
{
  "path": "$.statistics",
  "value": {
    "mean": "=avg($.data[*])",
    "variance": "=avg(pow($.data[*] - avg($.data[*]), 2))",
    "standardDeviation": "=sqrt(avg(pow($.data[*] - avg($.data[*]), 2)))"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Square Root
```csharp
var script = new JLioScript()
    .Add(SqrtBuilders.Sqrt("$.area"))
    .OnPath("$.sideLength");
```

### Geometric Calculations
```csharp
var script = new JLioScript()
    .Add(PowBuilders.Pow("$.x", "2"))
    .OnPath("$.xSquared")
    .Add(PowBuilders.Pow("$.y", "2"))
    .OnPath("$.ySquared")
    .Add(SqrtBuilders.Sqrt("$.xSquared + $.ySquared"))
    .OnPath("$.distance");
```

### Statistical Analysis
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.values[*]"))
    .OnPath("$.mean")
    .Add(SumBuilders.Sum("pow($.values[*] - $.mean, 2)"))
    .OnPath("$.sumOfSquares")
    .Add(CalculateBuilders.Calculate("$.sumOfSquares / count($.values[*])"))
    .OnPath("$.variance")
    .Add(SqrtBuilders.Sqrt("$.variance"))
    .OnPath("$.standardDeviation");
```

## Data Type Support

### Numeric Types
- **Integer**: `42` ? `6.480...`
- **Float/Double**: `3.14159` ? `1.772...`
- **Perfect Squares**: `4` ? `2.0`, `9` ? `3.0`

### String Numbers
Automatically converts using InvariantCulture:
```json
"=sqrt('25')"                // Result: 5.0
"=sqrt('2.25')"              // Result: 1.5
"=sqrt('0')"                 // Result: 0.0
```

### Special Cases
```json
"=sqrt(0)"                   // Result: 0
"=sqrt(1)"                   // Result: 1
"=sqrt(0.25)"                // Result: 0.5
"=sqrt(100)"                 // Result: 10
```

### Null Handling
- **null values**: Treated as `0`, result is `0`

## Mathematical Behavior

### Domain Restrictions
- **Non-negative only**: Input must be ? 0
- **Negative inputs**: Will cause mathematical error

### Precision
- **Perfect squares**: Return exact integer values when possible
- **Irrational results**: Return approximations with double precision
- **Small values**: Maintain precision for values close to zero

## Error Handling

### Argument Validation
```json
// Correct usage
"=sqrt(16)"          // ? Result: 4
"=sqrt('25')"        // ? Result: 5
"=sqrt($.value)"     // ? Uses JSONPath value

// Errors
"=sqrt()"            // ? Missing required argument
"=sqrt(-4)"          // ? Negative number (complex result)
"=sqrt(1, 2)"        // ? Too many arguments
"=sqrt('text')"      // ? Non-numeric string
"=sqrt([])"          // ? Array not supported
```

### Error Messages
- **Missing arguments**: "sqrt requires exactly one argument"
- **Invalid types**: "sqrt can only handle numeric values"
- **Negative input**: "sqrt cannot process negative numbers"

## Use Cases

### Geometric Applications
1. **Distance Calculations**: Euclidean distance between points
2. **Area/Side Relationships**: Find side from area of squares
3. **Pythagorean Theorem**: Calculate hypotenuse lengths
4. **Circle Calculations**: Radius from area relationships

### Statistical Analysis
1. **Standard Deviation**: Calculate spread of data
2. **Root Mean Square**: RMS calculations
3. **Variance Analysis**: Convert variance to standard deviation
4. **Error Analysis**: Calculate standard errors

### Engineering
1. **Signal Processing**: RMS voltage calculations  
2. **Physics**: Velocity, acceleration calculations
3. **Quality Control**: Standard deviation in measurements
4. **Optimization**: Distance minimization problems

## Performance Considerations

- **Execution Speed**: Optimized mathematical operation
- **Precision**: Uses double-precision floating point
- **Memory Usage**: Minimal memory footprint
- **Error Checking**: Validates non-negative input

## Integration Examples

### 2D Distance Calculation
```json
{
  "path": "$.distanceCalculation",
  "value": {
    "deltaX": "=abs($.point1.x - $.point2.x)",
    "deltaY": "=abs($.point1.y - $.point2.y)",
    "distance": "=sqrt(pow($.deltaX, 2) + pow($.deltaY, 2))"
  },
  "command": "add"
}
```

### Statistical Measures
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.measurements[*]"))
    .OnPath("$.mean")
    .Add(AvgBuilders.Avg("pow($.measurements[*] - $.mean, 2)"))
    .OnPath("$.variance")
    .Add(SqrtBuilders.Sqrt("$.variance"))
    .OnPath("$.standardDeviation")
    .Add(CalculateBuilders.Calculate("$.standardDeviation / $.mean * 100"))
    .OnPath("$.coefficientOfVariation");
```

### Quality Control Analysis
```json
[
  {
    "path": "$.qualityMetrics.sampleMean",
    "value": "=avg($.samples[*])",
    "command": "add"
  },
  {
    "path": "$.qualityMetrics.sampleVariance", 
    "value": "=sum(pow($.samples[*] - $.qualityMetrics.sampleMean, 2)) / (count($.samples[*]) - 1)",
    "command": "add"
  },
  {
    "path": "$.qualityMetrics.sampleStdDev",
    "value": "=sqrt($.qualityMetrics.sampleVariance)",
    "command": "add"
  },
  {
    "path": "$.qualityMetrics.controlLimits",
    "value": {
      "upper": "=$.qualityMetrics.sampleMean + 3 * $.qualityMetrics.sampleStdDev",
      "lower": "=$.qualityMetrics.sampleMean - 3 * $.qualityMetrics.sampleStdDev"
    },
    "command": "add"
  }
]
```

## Related Functions

- **[pow](pow.md)**: Power function (sqrt is equivalent to `pow(x, 0.5)`)
- **[abs](abs.md)**: Absolute value (useful before sqrt for safety)
- **[round](round.md)**: Round sqrt results for precision
- **[avg](avg.md)**: Average (used in variance calculations)
- **[sum](sum.md)**: Sum (used in statistical formulas)

## Mathematical Relationships

### Relationship to Powers
```json
"=sqrt(x)"                   // Equivalent to pow(x, 0.5)
"=pow(sqrt(x), 2)"          // Equals x (for x ? 0)
"=sqrt(pow(x, 2))"          // Equals abs(x)
```

### Statistical Relationships
```json
// Standard deviation formula
"=sqrt(variance)"

// Root mean square
"=sqrt(avg(pow(values[*], 2)))"

// Standard error of mean
"=sqrt(variance / n)"
```

## Best Practices

### Input Validation
```csharp
// Ensure non-negative input
var script = new JLioScript()
    .Add(SqrtBuilders.Sqrt("$.value"))
    .OnPath("$.result")
    .ConditionalOn("$.value >= 0");
```

### Handle Edge Cases
```json
{
  "path": "$.safeSquareRoot",
  "command": "ifElse",
  "condition": "$.value >= 0",
  "then": [
    {
      "path": "$.result",
      "value": "=sqrt($.value)",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result",
      "value": "Invalid: negative input",
      "command": "add"
    }
  ]
}
```

### Precision Control
```csharp
// Round results for display
var script = new JLioScript()
    .Add(RoundBuilders.Round("sqrt($.value)", "4"))
    .OnPath("$.preciseResult");
```

## Advanced Usage Patterns

### Multi-Dimensional Distance
```json
{
  "path": "$.3DDistance",
  "value": "=sqrt(pow($.x2 - $.x1, 2) + pow($.y2 - $.y1, 2) + pow($.z2 - $.z1, 2))",
  "command": "add"
}
```

### Portfolio Risk Calculation
```csharp
var script = new JLioScript()
    // Calculate portfolio variance (simplified)
    .Add(SumBuilders.Sum("pow($.weights[*] * $.volatilities[*], 2)"))
    .OnPath("$.portfolioVariance")
    // Portfolio volatility is sqrt of variance
    .Add(SqrtBuilders.Sqrt("$.portfolioVariance"))
    .OnPath("$.portfolioVolatility");
```

### Signal Processing
```json
{
  "path": "$.signalAnalysis",
  "value": {
    "meanSquare": "=avg(pow($.signal[*], 2))",
    "rmsValue": "=sqrt(avg(pow($.signal[*], 2)))",
    "crestFactor": "=max(abs($.signal[*])) / sqrt(avg(pow($.signal[*], 2)))"
  },
  "command": "add"
}
```

### Geometric Optimization
```json
{
  "path": "$.optimization",
  "value": {
    "circleRadius": "=sqrt($.area / 3.14159)",
    "sphereRadius": "=pow(3 * $.volume / (4 * 3.14159), 1/3)",
    "squareSide": "=sqrt($.area)",
    "cubeEdge": "=pow($.volume, 1/3)"
  },
  "command": "add"
}
```

## Common Pitfalls

### Negative Input Protection
```json
// Always validate input is non-negative
{
  "path": "$.calculation",
  "command": "ifElse",
  "condition": "$.inputValue >= 0",
  "then": [
    {
      "path": "$.result",
      "value": "=sqrt(abs($.inputValue))",  // abs() for extra safety
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.error",
      "value": "Cannot calculate square root of negative number",
      "command": "add"
    }
  ]
}
```

### Precision Expectations
```csharp
// Be aware of floating point precision
var script = new JLioScript()
    .Add(SqrtBuilders.Sqrt("2"))
    .OnPath("$.sqrtTwo");  // Results in 1.4142135623730951 (approximation)
```

### Inverse Operations
```json
{
  "path": "$.verification",
  "value": {
    "original": "$.value",
    "squared": "=pow($.value, 2)",
    "backToOriginal": "=sqrt(pow($.value, 2))",  // Should equal abs($.value)
    "isEqual": "=abs($.value - sqrt(pow($.value, 2))) < 0.0001"
  },
  "command": "add"
}
```

### Domain Awareness
```csharp
// Remember that sqrt has a restricted domain
var script = new JLioScript()
    // Good: ensure positive input
    .Add(SqrtBuilders.Sqrt("abs($.variance)"))
    .OnPath("$.standardDeviation")
    
    // Risky: could be negative
    // .Add(SqrtBuilders.Sqrt("$.someCalculation"))
```

### ? What Actually Works - Single Values Only:
The sqrt function accepts **single numeric values only**:

```json
"=sqrt(9)"              // ? Literal number
"=sqrt($.value)"        // ? JSONPath to number  
"=sqrt($.array[0])"     // ? Specific array element
```

### ? What Doesn't Work - Arithmetic Expressions:
```json
"=sqrt($.a / $.b)"                    // ? Division not supported
"=sqrt(pow($.x, 2) + pow($.y, 2))"    // ? Function nesting not supported
"=sqrt($.area / 3.14159)"             // ? Arithmetic not supported
"=sqrt(3 * $.volume / (4 * 3.14159))" // ? Complex expressions not supported
```

### ? Use Multi-Step Approach for Complex Calculations:

**Pythagorean Theorem - Correct Approach:**
```json
[
  {
    "path": "$.sideASquared",
    "value": "=pow($.sideA, 2)",
    "command": "add"
  },
  {
    "path": "$.sideBSquared", 
    "value": "=pow($.sideB, 2)",
    "command": "add"
  },
  {
    "path": "$.sumOfSquares",
    "value": "=calculate('{{$.sideASquared}} + {{$.sideBSquared}}')",
    "command": "add"
  },
  {
    "path": "$.hypotenuse",
    "value": "=sqrt($.sumOfSquares)",
    "command": "add"
  }
]
```

**Circle Radius from Area - Correct Approach:**
```json
[
  {
    "path": "$.areaOverPi",
    "value": "=calculate('{{$.area}} / 3.14159')",
    "command": "add"
  },
  {
    "path": "$.circleRadius",
    "value": "=sqrt($.areaOverPi)",
    "command": "add"
  }
]