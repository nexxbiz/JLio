# pow Function

Raise a number to a specified power (exponentiation).

## Overview

The `pow` function raises a base number to a specified exponent power, supporting numbers, numeric strings, and JSONPath expressions. It's essential for mathematical calculations, statistical analysis, and engineering computations.

## Syntax

### Expression Format
```json
"=pow(base, exponent)"
```

### Builder Pattern
```csharp
PowBuilders.Pow(base, exponent)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| base | number/string | Yes | The base number to be raised to a power |
| exponent | number/string | Yes | The power to which the base is raised |

## Examples

### Basic Exponentiation
```json
{
  "path": "$.result",
  "value": "=pow(2, 3)",
  "command": "add"
}
```

**Result**: `8` (2³ = 8)

### Square Calculations
```json
{
  "path": "$.area",
  "value": "=pow($.sideLength, 2)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "sideLength": 5
}
```

**Result**: `25` (5² = 25)

### String Number Support
```json
{
  "path": "$.power",
  "value": "=pow('3.5', '2')",
  "command": "add"
}
```

**Result**: `12.25`

### Scientific Calculations
```json
{
  "path": "$.physics",
  "value": {
    "kineticEnergy": "=0.5 * $.mass * pow($.velocity, 2)",
    "area": "=3.14159 * pow($.radius, 2)",
    "volume": "=(4/3) * 3.14159 * pow($.radius, 3)",
    "exponentialGrowth": "=$.initial * pow($.growthRate, $.time)"
  },
  "command": "add"
}
```

### Compound Interest
```json
{
  "path": "$.compoundInterest",
  "value": "=$.principal * pow(1 + $.rate, $.years)",
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Power Calculation
```csharp
var script = new JLioScript()
    .Add(PowBuilders.Pow("$.base", "2"))  // Square
    .OnPath("$.squared");
```

### Compound Calculations
```csharp
var script = new JLioScript()
    .Add(PowBuilders.Pow("$.radius", "2"))
    .OnPath("$.radiusSquared")
    .Add(CalculateBuilders.Calculate("3.14159 * $.radiusSquared"))
    .OnPath("$.circleArea");
```

### Statistical Calculations
```csharp
var script = new JLioScript()
    .Add(PowBuilders.Pow("$.deviation", "2"))
    .OnPath("$.variance")
    .Add(SqrtBuilders.Sqrt("$.variance"))
    .OnPath("$.standardDeviation");
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float/Double**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts using InvariantCulture:
```json
"=pow('2', '3')"             // Result: 8
"=pow('1.5', '2')"           // Result: 2.25
"=pow('10', '0.5')"          // Result: 3.162... (square root of 10)
```

### Special Cases
```json
"=pow(2, 0)"                 // Result: 1 (any number to power 0)
"=pow(0, 2)"                 // Result: 0 (zero to any positive power)
"=pow(1, 100)"               // Result: 1 (one to any power)
"=pow(-2, 2)"                // Result: 4 (negative base, even exponent)
"=pow(-2, 3)"                // Result: -8 (negative base, odd exponent)
```

### Fractional Exponents
```json
"=pow(9, 0.5)"               // Result: 3 (square root)
"=pow(8, 1/3)"               // Result: 2 (cube root)
"=pow(16, 0.25)"             // Result: 2 (fourth root)
```

## Mathematical Behavior

### Power Rules
- **Zero exponent**: `pow(a, 0) = 1` (for a ? 0)
- **Unity base**: `pow(1, n) = 1`
- **Zero base**: `pow(0, n) = 0` (for n > 0)
- **Negative exponents**: `pow(a, -n) = 1 / pow(a, n)`

### Sign Rules
- **Positive base**: Always positive result
- **Negative base, even exponent**: Positive result
- **Negative base, odd exponent**: Negative result

## Error Handling

### Argument Validation
```json
// Correct usage
"=pow(2, 3)"         // ? Result: 8
"=pow('2', '3')"     // ? Result: 8
"=pow($.base, $.exp)" // ? Uses JSONPath values

// Potential errors
"=pow(2)"            // ? Missing exponent argument
"=pow()"             // ? Missing both arguments
"=pow(1, 2, 3)"      // ? Too many arguments
"=pow('text', 2)"    // ? Non-numeric base
"=pow(0, -1)"        // ? Zero to negative power (undefined)
```

### Error Messages
- **Wrong argument count**: "pow requires exactly two arguments"
- **Invalid types**: "pow can only handle numeric values"
- **Mathematical errors**: "Cannot raise zero to negative power"

## Use Cases

### Scientific Applications
1. **Physics Calculations**: Energy, force, motion equations
2. **Engineering**: Stress, strain, material properties
3. **Statistics**: Variance, standard deviation calculations
4. **Chemistry**: Concentration, reaction rate calculations

### Financial Modeling
1. **Compound Interest**: Investment growth calculations
2. **Present Value**: Discounting future cash flows
3. **Growth Projections**: Exponential growth models
4. **Risk Analysis**: Volatility and variance calculations

### Business Analytics
1. **Scaling Calculations**: Non-linear growth models
2. **Performance Metrics**: Quadratic scoring systems
3. **Capacity Planning**: Exponential demand growth
4. **Quality Metrics**: Error rate calculations

## Performance Considerations

- **Execution Speed**: Optimized mathematical operation
- **Precision**: Uses double-precision floating point
- **Large Exponents**: May cause overflow for large values
- **Fractional Exponents**: More computationally intensive than integer exponents

## Integration Examples

### Geometric Calculations
```json
{
  "path": "$.geometry",
  "value": {
    "squareArea": "=pow($.side, 2)",
    "cubeVolume": "=pow($.edge, 3)",
    "circleArea": "=3.14159 * pow($.radius, 2)",
    "sphereVolume": "=(4/3) * 3.14159 * pow($.radius, 3)"
  },
  "command": "add"
}
```

### Financial Modeling
```csharp
var script = new JLioScript()
    .Add(PowBuilders.Pow("1 + $.interestRate", "$.years"))
    .OnPath("$.growthFactor")
    .Add(CalculateBuilders.Calculate("$.principal * $.growthFactor"))
    .OnPath("$.futureValue")
    .Add(SubtractBuilders.Subtract("$.futureValue", "$.principal"))
    .OnPath("$.totalInterest");
```

### Statistical Analysis
```json
[
  {
    "path": "$.statistics.mean",
    "value": "=avg($.data[*])",
    "command": "add"
  },
  {
    "path": "$.statistics.deviations[*]",
    "value": "=pow(@ - $.statistics.mean, 2)",
    "command": "set"
  },
  {
    "path": "$.statistics.variance",
    "value": "=avg($.statistics.deviations[*])",
    "command": "add"
  },
  {
    "path": "$.statistics.standardDeviation",
    "value": "=sqrt($.statistics.variance)",
    "command": "add"
  }
]
```

## Related Functions

- **[sqrt](sqrt.md)**: Square root (equivalent to `pow(x, 0.5)`)
- **[abs](abs.md)**: Absolute value (useful with negative bases)
- **[calculate](calculate.md)**: Complex expressions using power operations
- **[round](round.md)**: Round power results for precision
- **[min](min.md)**: Find minimum (power can create large values)
- **[max](max.md)**: Find maximum (power can create large values)

## Mathematical Relationships

### Roots as Powers
```json
"=pow(x, 0.5)"               // Square root of x
"=pow(x, 1/3)"               // Cube root of x  
"=pow(x, 1/n)"               // nth root of x
```

### Logarithmic Relationship
```json
// If y = pow(a, x), then x = log_a(y)
"=pow(2, 3)"                 // Result: 8
// log_2(8) = 3
```

## Best Practices

### Handle Large Results
```csharp
// Be careful with large exponents
var script = new JLioScript()
    .Add(PowBuilders.Pow("$.base", "$.exponent"))
    .OnPath("$.result")
    .ConditionalOn("$.exponent <= 100");  // Prevent overflow
```

### Precision Considerations
```json
{
  "path": "$.preciseCalculation",
  "value": "=round(pow($.base, $.exponent), 6)",
  "command": "add"
}
```

### Financial Applications
```csharp
// Use appropriate precision for financial calculations
var script = new JLioScript()
    .Add(RoundBuilders.Round("pow(1 + $.rate, $.periods)", "8"))
    .OnPath("$.growthFactor")
    .Add(RoundBuilders.Round("$.principal * $.growthFactor", "2"))
    .OnPath("$.futureValue");
```

## Advanced Usage Patterns

### Compound Growth Models
```json
{
  "path": "$.growthProjection",
  "value": {
    "year1": "=$.initial * pow($.growthRate, 1)",
    "year2": "=$.initial * pow($.growthRate, 2)",
    "year3": "=$.initial * pow($.growthRate, 3)",
    "year5": "=$.initial * pow($.growthRate, 5)",
    "year10": "=$.initial * pow($.growthRate, 10)"
  },
  "command": "add"
}
```

### Statistical Calculations
```csharp
var script = new JLioScript()
    // Calculate sum of squares
    .Add(SumBuilders.Sum("pow($.data[*], 2)"))
    .OnPath("$.sumOfSquares")
    // Calculate mean of squares
    .Add(CalculateBuilders.Calculate("$.sumOfSquares / count($.data[*])"))
    .OnPath("$.meanSquare")
    // Root mean square
    .Add(SqrtBuilders.Sqrt("$.meanSquare"))
    .OnPath("$.rms");
```

### Engineering Applications
```json
{
  "path": "$.engineering",
  "value": {
    "force": "=$.mass * pow($.acceleration, 1)",
    "energy": "=0.5 * $.mass * pow($.velocity, 2)",
    "pressure": "=$.force / pow($.area, 1)",
    "volume": "=pow($.length, 3)"
  },
  "command": "add"
}
```

### Conditional Power Calculations
```json
{
  "path": "$.scaledValue",
  "command": "ifElse",
  "condition": "$.useExponentialScaling == true",
  "then": [
    {
      "path": "$.result",
      "value": "=pow($.baseValue, $.scalingFactor)",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result", 
      "value": "=$.baseValue * $.scalingFactor",
      "command": "add"
    }
  ]
}
```

## Common Pitfalls

### Overflow Protection
```json
// Protect against large results
{
  "path": "$.safeCalculation",
  "command": "ifElse",
  "condition": "$.exponent > 10",
  "then": [
    {
      "path": "$.result",
      "value": "OVERFLOW_RISK",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result",
      "value": "=pow($.base, $.exponent)",
      "command": "add"
    }
  ]
}
```

### Zero Base Handling
```csharp
// Handle zero base with negative exponents
var script = new JLioScript()
    .Add(PowBuilders.Pow("$.base", "$.exponent"))
    .OnPath("$.result")
    .ConditionalOn("$.base != 0 || $.exponent >= 0");
```

### Precision Loss
```json
{
  "path": "$.calculations",
  "value": {
    // May lose precision with repeated operations
    "imprecise": "=pow(pow($.value, 0.5), 2)",  // Should equal $.value but may not
    // Better: avoid unnecessary round trips
    "precise": "=abs($.value)"  // Use abs instead for this case
  },
  "command": "add"
}
```

### ? What Doesn't Work - Arithmetic Expressions:
Individual math functions **do not support arithmetic expressions**:

```json
"=pow($.a * 2, $.b)"           // ? Arithmetic not supported
"=pow($.base + 1, $.exp)"      // ? Expressions not supported  
"=sqrt($.a / $.b)"             // ? Division not supported
"=abs($.x - $.y)"              // ? Subtraction not supported
```

### ? Use Calculate for Arithmetic + Math Functions:
For complex expressions with arithmetic, use the **two-step approach**:

```json
[
  {
    "path": "$.intermediate",
    "value": "=calculate('0.5 * {{$.mass}} * {{$.velocity}} * {{$.velocity}}')",
    "command": "add"
  },
  {
    "path": "$.kineticEnergy",
    "value": "=pow($.intermediate, 1)",
    "command": "add"
  }
]
```

**Or break into multiple steps:**

```json
[
  {
    "path": "$.velocitySquared",
    "value": "=pow($.velocity, 2)",
    "command": "add"
  },
  {
    "path": "$.kineticEnergy",
    "value": "=calculate('0.5 * {{$.mass}} * {{$.velocitySquared}}')",
    "command": "add"
  }
]
```

### Working Examples - Single Values Only:
```json
{
  "path": "$.calculations",
  "value": {
    "squareArea": "=pow($.side, 2)",              // ? Single value
    "cubeVolume": "=pow($.edge, 3)",              // ? Single value
    "squareRoot": "=sqrt($.area)",                // ? Single value
    "absoluteValue": "=abs($.difference)"         // ? Single value
  },
  "command": "add"
}