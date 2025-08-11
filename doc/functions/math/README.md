# Math Functions Documentation

This directory contains comprehensive documentation for all JLio mathematical functions.

## Available Functions

### Basic Arithmetic
- [sum](sum.md) - Sum numeric values including arrays and string numbers
- [subtract](subtract.md) - Subtract one value from another
- [avg](avg.md) - Calculate arithmetic mean of numeric values

### Statistical Functions
- [min](min.md) - Find minimum value from multiple inputs
- [max](max.md) - Find maximum value from multiple inputs
- [median](median.md) - Calculate median value from numeric dataset
- [count](count.md) - Count elements in arrays or objects

### Mathematical Operations
- [abs](abs.md) - Calculate absolute value
- [pow](pow.md) - Raise number to a power
- [sqrt](sqrt.md) - Calculate square root
- [round](round.md) - Round numbers to specified decimal places
- [floor](floor.md) - Round down to nearest integer
- [ceiling](ceiling.md) - Round up to nearest integer

### Advanced Calculations
- [calculate](calculate.md) - Evaluate complex mathematical expressions

## Installation

### Extension Pack Registration
```csharp
// Register math functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterMath();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Key Features

### String Number Support
All math functions automatically convert string representations of numbers:
```json
"=sum(10, '20', 30)"           // Result: 60
"=avg(100, '25.50', 50)"       // Result: 58.5
"=max('3.14', 2, '5.7')"       // Result: 5.7
```

### Array Processing
Most functions can process arrays recursively:
```json
"=sum($.numbers[*])"           // Sum all array elements
"=avg($.scores[*])"            // Average all scores
"=max($.values[*])"            // Find maximum value
```

### JSONPath Integration
All functions work seamlessly with JSONPath expressions:
```json
"=sum($.products[*].price)"    // Sum all product prices
"=avg($.students[*].grade)"    // Average student grades
"=count($.items[*])"           // Count array items
```

### Cultural Independence
All functions use `InvariantCulture` for consistent number parsing across different system locales.

## Usage Patterns

### Basic Calculations
```json
{
  "path": "$.total",
  "value": "=sum($.basePrice, $.tax, $.shipping)",
  "command": "add"
}
```

### Statistical Analysis
```json
{
  "path": "$.statistics",
  "value": {
    "average": "=avg($.scores[*])",
    "minimum": "=min($.scores[*])",
    "maximum": "=max($.scores[*])",
    "count": "=count($.scores[*])"
  },
  "command": "add"
}
```

### Financial Calculations
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.lineItems[*].amount"))
    .OnPath("$.subtotal")
    .Add(SumBuilders.Sum("$.subtotal", "$.tax", "$.shipping"))
    .OnPath("$.total")
    .Add(RoundBuilders.Round("$.total", "2"))
    .OnPath("$.roundedTotal");
```

### Data Aggregation
```json
[
  {
    "path": "$.metrics.totalRevenue",
    "value": "=sum($.orders[*].amount)",
    "command": "add"
  },
  {
    "path": "$.metrics.averageOrderValue",
    "value": "=avg($.orders[*].amount)",
    "command": "add"
  },
  {
    "path": "$.metrics.orderCount",
    "value": "=count($.orders[*])",
    "command": "add"
  }
]
```

## Common Use Cases

### E-commerce
- Order totals and subtotals
- Tax calculations
- Discount applications
- Price aggregations

### Analytics
- Statistical summaries
- Performance metrics
- Data analysis
- Reporting calculations

### Financial
- Budget calculations
- Cost analysis
- Revenue reporting
- Interest calculations

### Scientific
- Mathematical modeling
- Data processing
- Statistical analysis
- Engineering calculations

## Error Handling

All math functions include comprehensive error handling:
- Type validation for numeric inputs
- Graceful handling of non-numeric strings
- Division by zero protection
- Array processing error management
- Detailed logging for debugging

## Performance Considerations

- Functions are optimized for common operations
- Array processing scales linearly with data size
- String-to-number conversion adds minimal overhead
- Memory usage is optimized for large datasets

## Best Practices

1. **Data Validation**: Ensure numeric data quality before processing
2. **Performance**: Use specific JSONPath expressions for better performance  
3. **Precision**: Consider floating-point precision for financial calculations
4. **Error Monitoring**: Check execution logs for processing errors
5. **Testing**: Test with various data scenarios including edge cases
6. **Documentation**: Document expected data ranges and precision requirements