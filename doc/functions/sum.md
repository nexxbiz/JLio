# Sum Function Documentation

## Overview

The `Sum` function calculates the total of numeric values from multiple sources including literal values, JSONPath expressions, and arrays. It supports complex nested data structures and automatically handles type conversion for string numbers.

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
// Sum literal values
"=sum(1, 2, 3)"

// Sum from JSONPath expressions
"=sum($.price, $.tax, $.shipping)"

// Sum array elements
"=sum($.numbers[*])"

// Sum nested array elements
"=sum($.regions[*].stores[*].revenue)"

// Mixed arguments
"=sum(10, $.amount, $.fees[*])"
```

### Programmatic Usage
```csharp
// With literal arguments
var sumFunction = new Sum("1", "2", "3");

// Empty constructor for dynamic arguments
var sumFunction = new Sum();
```

### Builder Pattern
```csharp
var sumFunction = SumBuilders.Sum("1", "2", "3");
var pathSum = SumBuilders.Sum("$.price", "$.tax");
```

## Parameters

- **Multiple Arguments**: Accepts any number of numeric arguments
- **Argument Types**: 
  - Literal numbers (integers, floats)
  - String numbers (automatically parsed)
  - JSONPath expressions
  - Array references
- **Return Type**: Double (JValue)

## Examples

### Basic Numeric Addition
```json
{
  "path": "$.total",
  "value": "=sum($.basePrice, $.tax, $.shipping)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "basePrice": 100.00,
  "tax": 8.50,
  "shipping": 15.00
}
```

**Result**:
```json
{
  "basePrice": 100.00,
  "tax": 8.50,
  "shipping": 15.00,
  "total": 123.50
}
```

### Array Summation
```json
{
  "path": "$.totalScore",
  "value": "=sum($.scores[*])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "scores": [85, 92, 78, 96]
}
```

**Result**:
```json
{
  "scores": [85, 92, 78, 96],
  "totalScore": 351
}
```

### Object Property Summation
```json
{
  "path": "$.totalPayroll",
  "value": "=sum($.employees[*].salary)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "employees": [
    {"name": "John", "salary": 50000},
    {"name": "Jane", "salary": 60000},
    {"name": "Bob", "salary": 45000}
  ]
}
```

**Result**:
```json
{
  "employees": [...],
  "totalPayroll": 155000
}
```

### Nested Structure Summation
```json
{
  "path": "$.totalRevenue",
  "value": "=sum($.regions[*].stores[*].revenue)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "regions": [
    {
      "name": "North",
      "stores": [
        {"id": 1, "revenue": 10000},
        {"id": 2, "revenue": 15000}
      ]
    },
    {
      "name": "South",
      "stores": [
        {"id": 3, "revenue": 12000},
        {"id": 4, "revenue": 8000}
      ]
    }
  ]
}
```

**Result**:
```json
{
  "regions": [...],
  "totalRevenue": 45000
}
```

### Mixed Type Summation
```json
{
  "path": "$.grandTotal",
  "value": "=sum(100, $.stringNumber, $.arrayNumbers[*])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "stringNumber": "25.50",
  "arrayNumbers": [10, 15, 20]
}
```

**Result**:
```json
{
  "stringNumber": "25.50",
  "arrayNumbers": [10, 15, 20],
  "grandTotal": 170.50
}
```

### Complex Business Logic
```json
{
  "path": "$.orderTotal",
  "value": "=sum($.products[*].price, $.services[*].cost, $.fees.processing, $.fees.shipping)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "products": [
    {"name": "Laptop", "price": 999.99},
    {"name": "Mouse", "price": 29.99}
  ],
  "services": [
    {"type": "Setup", "cost": 50.00},
    {"type": "Training", "cost": 100.00}
  ],
  "fees": {
    "processing": 15.99,
    "shipping": 25.00
  }
}
```

**Result**:
```json
{
  "products": [...],
  "services": [...],
  "fees": {...},
  "orderTotal": 1220.97
}
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts string representations:
```json
"=sum('10', '20.5', '30')"  // Result: 60.5
```

### Arrays
Processes arrays recursively:
```json
// Flat array
"=sum([1, 2, 3])"  // Result: 6

// Nested arrays  
"=sum([[1, 2], [3, 4]])"  // Result: 10

// Object arrays
"=sum([{value: 10}, {value: 20}])"  // Sums 'value' properties
```

## Advanced Usage

### Financial Calculations
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.lineItems[*].amount"))
    .OnPath("$.subtotal")
    .Add(SumBuilders.Sum("$.subtotal", "$.taxes[*].amount", "$.fees[*].amount"))
    .OnPath("$.total");
```

### Inventory Valuation
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.inventory[*].quantity"))
    .OnPath("$.totalQuantity")
    .Add(SumBuilders.Sum("$.inventory[*].value"))
    .OnPath("$.totalValue");
```

### Performance Metrics
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.metrics[*].requests"))
    .OnPath("$.totalRequests")
    .Add(SumBuilders.Sum("$.metrics[*].responseTime"))
    .OnPath("$.totalResponseTime");
```

## Fluent API Usage

### Basic Summation
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("1", "2", "3"))
    .OnPath("$.result");
```

### Path-Based Summation
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.price", "$.tax"))
    .OnPath("$.total");
```

### Complex Calculations
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.orders[*].total"))
    .OnPath("$.totalSales")
    .Add(SumBuilders.Sum("$.totalSales", "$.adjustments[*].amount"))
    .OnPath("$.finalTotal");
```

## Error Handling

### Invalid Data Types
```json
{
  "path": "$.result",
  "value": "=sum($.invalidValue)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "invalidValue": "not-a-number"
}
```

**Result**: Function fails and logs error
```
Log: "sum can only handle numeric values or arrays. Current type = String"
```

### Mixed Valid and Invalid Types
If any argument contains invalid data, the entire function fails:
```json
"=sum(1, 2, 'invalid')"  // Fails due to invalid string
```

### Empty Input
```json
"=sum()"  // Returns: 0
```

### Null Values
Null values are skipped in calculations:
```json
"=sum(1, null, 3)"  // Returns: 4
```

## Performance Considerations

### Large Arrays
- Memory usage scales with array size
- Consider breaking very large calculations into chunks
- Use specific JSONPath expressions rather than broad wildcards

### Nested Structures
- Deep nesting requires more processing time
- JSONPath expressions like `$..value` traverse entire structures
- Prefer specific paths like `$.items[*].value` for better performance

### Type Conversion
- String-to-number conversion has minimal overhead
- Type checking performed for each value
- Early termination on invalid types

## Best Practices

1. **Validate Input Data**: Ensure all values are numeric before calculation
2. **Use Specific Paths**: Prefer `$.items[*].price` over `$..price` for performance
3. **Handle Edge Cases**: Plan for empty arrays and null values
4. **Error Checking**: Always verify function execution success
5. **Data Quality**: Validate numeric data consistency
6. **Memory Awareness**: Consider memory usage with large datasets
7. **Testing**: Test with realistic data volumes and edge cases

## Integration Examples

### E-commerce Order Processing
```csharp
var script = new JLioScript()
    // Calculate item subtotal
    .Add(SumBuilders.Sum("$.items[*].price"))
    .OnPath("$.itemsSubtotal")
    // Add shipping and taxes
    .Add(SumBuilders.Sum("$.itemsSubtotal", "$.shipping", "$.taxes[*].amount"))
    .OnPath("$.orderTotal")
    // Apply discounts
    .Add(SumBuilders.Sum("$.discounts[*].amount"))
    .OnPath("$.totalDiscounts")
    // Final total
    .Add(SumBuilders.Sum("$.orderTotal", "=sum($.totalDiscounts) * -1"))
    .OnPath("$.finalTotal");
```

### Financial Reporting
```csharp
var script = new JLioScript()
    // Revenue calculations
    .Add(SumBuilders.Sum("$.sales[*].amount"))
    .OnPath("$.totalRevenue")
    // Cost calculations
    .Add(SumBuilders.Sum("$.expenses[*].amount"))
    .OnPath("$.totalExpenses")
    // Profit calculation
    .Add(SumBuilders.Sum("$.totalRevenue", "=sum($.totalExpenses) * -1"))
    .OnPath("$.netProfit");
```

### Resource Utilization
```csharp
var script = new JLioScript()
    // CPU usage across servers
    .Add(SumBuilders.Sum("$.servers[*].cpu.usage"))
    .OnPath("$.totalCpuUsage")
    // Memory usage
    .Add(SumBuilders.Sum("$.servers[*].memory.used"))
    .OnPath("$.totalMemoryUsed")
    // Storage usage
    .Add(SumBuilders.Sum("$.servers[*].storage[*].used"))
    .OnPath("$.totalStorageUsed");
```

## Common Patterns

### Subtotal Calculation Pattern
```json
"=sum($.items[*].amount)"
```

### Multi-Source Total Pattern
```json
"=sum($.primary, $.secondary[*], $.adjustments[*].value)"
```

### Nested Aggregation Pattern
```json
"=sum($.categories[*].products[*].price)"
```

### Conditional Summation Pattern
```json
"=sum($.transactions[?(@.status == 'completed')].amount)"
```