# round Function

Round numbers to a specified number of decimal places.

## Overview

The `round` function rounds numeric values to the nearest integer or to a specified number of decimal places. It uses standard mathematical rounding rules (0.5 rounds up) and is essential for financial calculations, data presentation, and precision control.

## Syntax

### Expression Format
```json
"=round(value)"           // Round to nearest integer
"=round(value, decimals)" // Round to specified decimal places
```

### Builder Pattern
```csharp
RoundBuilders.Round(value)
RoundBuilders.Round(value, decimals)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| value | number/string | Yes | The numeric value to round |
| decimals | integer | No | Number of decimal places (default: 0) |

## Examples

### Basic Rounding (to integer)
```json
{
  "path": "$.rounded",
  "value": "=round(4.6)",
  "command": "add"
}
```

**Result**: `5`

### Decimal Place Rounding
```json
{
  "path": "$.price",
  "value": "=round(123.4567, 2)",
  "command": "add"
}
```

**Result**: `123.46`

### String Number Conversion
```json
{
  "path": "$.amount",
  "value": "=round('45.789', 1)",
  "command": "add"
}
```

**Result**: `45.8`

### JSONPath Integration
```json
{
  "path": "$.averageRounded",
  "value": "=round(avg($.scores[*]), 1)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "scores": [87.3, 92.7, 88.1, 95.4]
}
```

**Result**: `90.9`

### Financial Calculations
```json
{
  "path": "$.invoice",
  "value": {
    "subtotal": "=round(sum($.items[*].price), 2)",
    "tax": "=round($.subtotal * $.taxRate, 2)",
    "total": "=round($.subtotal + $.tax, 2)"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Rounding
```csharp
var script = new JLioScript()
    .Add(RoundBuilders.Round("$.price"))
    .OnPath("$.wholePrice");
```

### Financial Precision
```csharp
var script = new JLioScript()
    .Add(RoundBuilders.Round("$.amount * $.taxRate", "2"))
    .OnPath("$.taxAmount")
    .Add(RoundBuilders.Round("$.amount + $.taxAmount", "2"))
    .OnPath("$.total");
```

### Statistical Rounding
```csharp
var script = new JLioScript()
    .Add(RoundBuilders.Round("avg($.values[*])", "3"))
    .OnPath("$.average")
    .Add(RoundBuilders.Round("sqrt(variance($.values[*]))", "4"))
    .OnPath("$.standardDeviation");
```

## Data Type Support

### Numeric Types
- **Integer**: May be unchanged if no rounding needed
- **Float/Double**: Rounded to specified precision
- **Decimal**: Maintains precision after rounding

### String Numbers
Automatically converts using InvariantCulture:
```json
"=round('3.14159', 2)"    // Result: 3.14
"=round('-2.789', 1)"     // Result: -2.8
```

### Null Values
Treats `null` as `0`:
```json
"=round(null)"     // Result: 0
"=round(null, 2)"  // Result: 0.00
```

## Rounding Behavior

### Standard Mathematical Rounding
- `0.5` and above rounds up
- Below `0.5` rounds down

### Examples by Decimal Places
```json
// No decimal places (integer)
"=round(3.2)"     // Result: 3
"=round(3.5)"     // Result: 4  
"=round(3.8)"     // Result: 4

// One decimal place
"=round(3.14, 1)" // Result: 3.1
"=round(3.15, 1)" // Result: 3.2
"=round(3.19, 1)" // Result: 3.2

// Two decimal places  
"=round(3.145, 2)" // Result: 3.15
"=round(3.146, 2)" // Result: 3.15 (some floating point nuances)
```

### Negative Numbers
```json
"=round(-3.2)"     // Result: -3
"=round(-3.5)"     // Result: -4 (rounds away from zero)
"=round(-3.8)"     // Result: -4
```

## Error Handling

### Argument Validation
```json
// Correct usage
"=round(3.14)"       // ? Result: 3
"=round(3.14, 2)"    // ? Result: 3.14

// Errors
"=round()"           // ? Missing required argument
"=round(1, 2, 3)"    // ? Too many arguments  
"=round('text')"     // ? Non-numeric string
"=round(3.14, 'x')"  // ? Non-numeric decimals parameter
```

### Decimal Places Validation
- **Negative decimals**: May cause errors or unexpected behavior
- **Very large decimals**: Limited by system precision
- **Non-integer decimals**: May be truncated to integer

## Use Cases

### Financial Applications
1. **Currency Calculations**: Round to 2 decimal places for currency
2. **Tax Calculations**: Precise tax amount calculations
3. **Interest Calculations**: Loan and investment calculations  
4. **Pricing**: Set precise product prices

### Statistical Analysis
1. **Report Generation**: Round statistics for presentation
2. **Data Visualization**: Prepare data for charts and graphs
3. **Performance Metrics**: Round KPIs and metrics
4. **Scientific Data**: Control measurement precision

### E-commerce
1. **Product Pricing**: Ensure consistent price formatting
2. **Discount Calculations**: Round discount amounts
3. **Shipping Costs**: Round calculated shipping fees
4. **Commission Calculations**: Round sales commissions

## Performance Considerations

- **Execution Speed**: Fast single-value operation
- **Decimal Precision**: More decimal places require more processing
- **String Conversion**: Minimal overhead with InvariantCulture
- **Memory Usage**: Efficient for standard numeric operations

## Integration Examples

### Complete Invoice Processing
```json
{
  "path": "$.processedInvoice",
  "value": {
    "subtotal": "=round(sum($.lineItems[*].amount), 2)",
    "discountAmount": "=round($.subtotal * $.discountRate, 2)", 
    "taxableAmount": "=round($.subtotal - $.discountAmount, 2)",
    "taxAmount": "=round($.taxableAmount * $.taxRate, 2)",
    "total": "=round($.taxableAmount + $.taxAmount, 2)"
  },
  "command": "add"
}
```

### Statistical Summary
```csharp
var script = new JLioScript()
    .Add(RoundBuilders.Round("avg($.data[*])", "2"))
    .OnPath("$.statistics.mean")
    .Add(RoundBuilders.Round("min($.data[*])", "2")) 
    .OnPath("$.statistics.minimum")
    .Add(RoundBuilders.Round("max($.data[*])", "2"))
    .OnPath("$.statistics.maximum")
    .Add(RoundBuilders.Round("median($.data[*])", "2"))
    .OnPath("$.statistics.median");
```

### Performance Metrics Dashboard
```json
[
  {
    "path": "$.metrics.responseTimeAvg",
    "value": "=round(avg($.responses[*].time), 1)", 
    "command": "add"
  },
  {
    "path": "$.metrics.throughputPerSec",
    "value": "=round(count($.requests[*]) / $.duration, 2)",
    "command": "add"
  },
  {
    "path": "$.metrics.errorRate",
    "value": "=round(count($.errors[*]) / count($.requests[*]) * 100, 2)",
    "command": "add"
  }
]
```

## Related Functions

- **[floor](floor.md)**: Always rounds down
- **[ceiling](ceiling.md)**: Always rounds up
- **[ceil](ceil.md)**: Alias for ceiling
- **[abs](abs.md)**: Absolute value
- **[avg](avg.md)**: Calculate averages (often needs rounding)

## Mathematical Properties

### Precision Control
Round controls the number of digits after the decimal point:
```json
"=round(3.14159, 0)"  // Result: 3 (integer)
"=round(3.14159, 1)"  // Result: 3.1
"=round(3.14159, 2)"  // Result: 3.14
"=round(3.14159, 3)"  // Result: 3.142
```

### Symmetry
Positive and negative numbers round symmetrically:
```json
"=round(3.5)"   // Result: 4
"=round(-3.5)"  // Result: -4
```

## Best Practices

### Financial Calculations
```csharp
// Always round financial calculations to 2 decimal places
var script = new JLioScript()
    .Add(RoundBuilders.Round("$.price * $.quantity", "2"))
    .OnPath("$.lineTotal")
    .Add(RoundBuilders.Round("$.lineTotal * $.taxRate", "2"))
    .OnPath("$.taxAmount");
```

### Avoid Over-Precision
```csharp
// Good: Appropriate precision for use case
.Add(RoundBuilders.Round("$.percentage", "1"))  // 99.5%

// Avoid: Unnecessary precision
.Add(RoundBuilders.Round("$.percentage", "8"))  // 99.50000000%
```

### Chain Rounding Carefully
```json
// Be careful with rounding intermediate results
{
  "path": "$.result1",
  "value": "=round($.value1 * $.multiplier, 2)",
  "command": "add"
},
{
  "path": "$.result2", 
  "value": "=round($.result1 * $.multiplier2, 2)",
  "command": "add"
}
```

## Advanced Usage Patterns

### Conditional Rounding
```json
{
  "path": "$.displayValue",
  "command": "ifElse",
  "condition": "=abs($.value) > 1000",
  "then": [
    {
      "path": "$.rounded",
      "value": "=round($.value, 0)", 
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.rounded",
      "value": "=round($.value, 2)",
      "command": "add"
    }
  ]
}
```

### Currency Formatting Pipeline
```csharp
var script = new JLioScript()
    // Calculate amount
    .Add(SumBuilders.Sum("$.items[*].price"))
    .OnPath("$.subtotal")
    // Apply discount
    .Add(RoundBuilders.Round("$.subtotal * (1 - $.discountRate)", "2"))
    .OnPath("$.discountedAmount")
    // Add tax
    .Add(RoundBuilders.Round("$.discountedAmount * (1 + $.taxRate)", "2"))
    .OnPath("$.finalAmount");
```

### Data Quality Processing
```json
{
  "path": "$.cleanedData[*]",
  "value": {
    "id": "@.id",
    "measurement": "=round(@.rawMeasurement, 3)",
    "percentage": "=round(@.rawPercentage, 1)",
    "currency": "=round(@.rawCurrency, 2)"
  },
  "command": "set"
}
```

## Common Pitfalls

### Floating Point Precision
```json
// Floating point arithmetic can be imprecise
"=round(0.1 + 0.2, 1)"  // Might not be exactly 0.3

// Better: Round after all calculations
"=round((0.1 + 0.2) * 10, 0) / 10"
```

### Rounding Too Early
```csharp
// Avoid: Rounding intermediate values
var script = new JLioScript()
    .Add(RoundBuilders.Round("$.price", "2"))
    .OnPath("$.roundedPrice")
    .Add(RoundBuilders.Round("$.roundedPrice * $.quantity", "2")) // Compounds rounding errors
    .OnPath("$.total");

// Better: Round final result
var script = new JLioScript()
    .Add(RoundBuilders.Round("$.price * $.quantity", "2"))
    .OnPath("$.total");
```