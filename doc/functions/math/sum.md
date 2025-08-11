# sum Function

Calculate the sum of multiple numeric values.

## Overview

The `sum` function adds multiple numeric values together, supporting numbers, numeric strings, arrays, and JSONPath expressions. It automatically handles type conversion and can recursively process nested arrays, making it ideal for aggregating data from complex JSON structures.

## Syntax

### Expression Format
```json
"=sum(value1, value2, ...)"
"=sum(array)"
"=sum($.path[*])"
```

### Builder Pattern
```csharp
SumBuilders.Sum(arguments...)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| values | number/string/array | Yes (1+) | One or more numeric values, arrays, or JSONPath expressions to sum |

## Examples

### Basic Usage
```json
{
  "path": "$.total",
  "value": "=sum(10, 20, 30)",
  "command": "add"
}
```

**Result**: `60`

### String Number Conversion
```json
{
  "path": "$.grandTotal",
  "value": "=sum(100, '25.50', 50)",
  "command": "add"
}
```

**Result**: `175.5`

### Array Processing
```json
{
  "path": "$.arraySum",
  "value": "=sum([1, 2, 3, 4, 5])",
  "command": "add"
}
```

**Result**: `15`

### JSONPath Integration
```json
{
  "path": "$.totalSales",
  "value": "=sum($.orders[*].amount)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "orders": [
    {"id": 1, "amount": 150.75},
    {"id": 2, "amount": 89.50},
    {"id": 3, "amount": 200.25}
  ]
}
```

**Result**: `440.5`

### Mixed Parameters
```json
{
  "path": "$.combinedTotal",
  "value": "=sum($.basePrice, $.taxes[*], $.fees.shipping, 10)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "basePrice": 100,
  "taxes": [5.50, 8.75],
  "fees": {"shipping": 15.25}
}
```

**Result**: `139.5` (100 + 5.50 + 8.75 + 15.25 + 10)

## Builder Pattern Usage

### Simple Sum
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.price", "$.tax", "$.shipping"))
    .OnPath("$.total");
```

### Complex Financial Calculations
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.lineItems[*].amount"))
    .OnPath("$.subtotal")
    .Add(SumBuilders.Sum("$.subtotal", "$.tax", "$.shipping", "$.handling"))
    .OnPath("$.grandTotal");
```

### Multi-Level Aggregation
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.departments[*].budget"))
    .OnPath("$.totalBudget")
    .Add(SumBuilders.Sum("$.departments[*].expenses[*]"))
    .OnPath("$.totalExpenses");
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float/Double**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts string representations using InvariantCulture:
```json
"=sum('10', '20.5', '30')"    // Result: 60.5
"=sum('-5', '3', '2')"        // Result: 0
```

### Arrays
Processes arrays recursively, including nested structures:
```json
// Flat array
"=sum([1, 2, 3])"             // Result: 6

// Nested arrays  
"=sum([[1, 2], [3, 4]])"      // Result: 10

// Mixed types in array
"=sum([1, '2.5', 3])"         // Result: 6.5
```

### Null and Undefined
- **null values**: Treated as `0`
- **undefined properties**: Ignored (not added to sum)

## Advanced Usage

### Financial Calculations
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.revenue[*]"))
    .OnPath("$.totalRevenue")
    .Add(SumBuilders.Sum("$.expenses[*].amount"))
    .OnPath("$.totalExpenses")
    .Add(SubtractBuilders.Subtract("$.totalRevenue", "$.totalExpenses"))
    .OnPath("$.netProfit");
```

### Inventory Management
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.warehouses[*].inventory[*].quantity"))
    .OnPath("$.totalInventory")
    .Add(SumBuilders.Sum("$.warehouses[*].inventory[*].value"))
    .OnPath("$.totalValue");
```

### Performance Analytics
```json
[
  {
    "path": "$.metrics.totalRequests",
    "value": "=sum($.servers[*].requests)",
    "command": "add"
  },
  {
    "path": "$.metrics.totalResponseTime", 
    "value": "=sum($.servers[*].responseTime)",
    "command": "add"
  },
  {
    "path": "$.metrics.averageResponseTime",
    "value": "=$.metrics.totalResponseTime / count($.servers[*])",
    "command": "add"
  }
]
```

## Error Handling

### Argument Validation
```json
// Correct usage
"=sum(1, 2, 3)"     // ? Result: 6
"=sum([1, 2, 3])"   // ? Result: 6
"=sum($.values[*])" // ? Sums array values

// Errors  
"=sum()"            // ? No arguments provided
"=sum('text')"      // ? Non-numeric string
"=sum({})"          // ? Object not supported
```

### Error Messages
- **No arguments**: "sum requires at least one argument"
- **Invalid types**: "sum can only handle numeric values or arrays. Current type = [Type]"
- **Parse failures**: Non-numeric strings result in detailed error logs

## Use Cases

### Business Applications
1. **Financial Reporting**: Calculate totals, subtotals, and aggregates
2. **Sales Analytics**: Sum revenue, commissions, and performance metrics
3. **Budget Management**: Aggregate departmental budgets and expenses
4. **Inventory Valuation**: Calculate total inventory values

### E-commerce
1. **Order Processing**: Calculate order totals with taxes and shipping
2. **Cart Management**: Sum product prices and fees
3. **Discount Calculation**: Apply and sum multiple discount amounts
4. **Revenue Analytics**: Aggregate sales across products and time periods

### Data Analysis
1. **Statistical Aggregation**: Sum data points for analysis
2. **Performance Metrics**: Aggregate system performance data
3. **Survey Results**: Sum numerical survey responses
4. **Scientific Calculations**: Aggregate experimental measurements

## Performance Considerations

- **Array Processing**: Linear time complexity O(n) for array elements
- **String Conversion**: Minimal overhead with optimized parsing
- **Memory Usage**: Efficient for large datasets with streaming processing
- **Nested Arrays**: Recursive processing scales with nesting depth

## Integration Examples

### Complete E-commerce Order
```json
{
  "path": "$.invoice",
  "value": {
    "subtotal": "=sum($.items[*].price)",
    "tax": "=sum($.items[*].tax)",
    "shipping": "=sum($.shippingMethods[*].cost)",
    "total": "=sum(sum($.items[*].price), sum($.items[*].tax), sum($.shippingMethods[*].cost))"
  },
  "command": "add"
}
```

### Multi-Currency Handling
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.transactions[?(@.currency == 'USD')].amount"))
    .OnPath("$.totalUSD")
    .Add(SumBuilders.Sum("$.transactions[?(@.currency == 'EUR')].amount"))
    .OnPath("$.totalEUR");
```

### Budget vs Actual Analysis
```json
[
  {
    "path": "$.analysis.totalBudget",
    "value": "=sum($.departments[*].budget)",
    "command": "add"
  },
  {
    "path": "$.analysis.totalActual",
    "value": "=sum($.departments[*].actual)",
    "command": "add"
  },
  {
    "path": "$.analysis.variance",
    "value": "=$.analysis.totalActual - $.analysis.totalBudget", 
    "command": "add"
  }
]
```

## Related Functions

- **[avg](avg.md)**: Calculate average of values
- **[count](count.md)**: Count number of elements
- **[min](min.md)**: Find minimum value
- **[max](max.md)**: Find maximum value
- **[subtract](subtract.md)**: Subtract values
- **[median](median.md)**: Find median value

## Mathematical Properties

### Commutative Property
Order doesn't matter: `sum(a, b, c) = sum(c, a, b)`

### Associative Property
Grouping doesn't matter: `sum(a, sum(b, c)) = sum(sum(a, b), c)`

### Identity Element
Zero doesn't change the sum: `sum(a, 0) = a`

### Array Flattening
Nested arrays are automatically flattened:
`sum([[1, 2], [3, 4]]) = sum([1, 2, 3, 4]) = 10`

## Best Practices

### Data Validation
```csharp
// Validate data before summing
var script = new JLioScript()
    .Add(FilterBuilders.Filter("$.values[*][@ != null]"))
    .OnPath("$.validValues")
    .Add(SumBuilders.Sum("$.validValues[*]"))
    .OnPath("$.total");
```

### Error Handling in Complex Calculations
```json
{
  "path": "$.safeTotal",
  "command": "ifElse",
  "condition": "=count($.values[*]) > 0",
  "then": [
    {
      "path": "$.result",
      "value": "=sum($.values[*])",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result", 
      "value": 0,
      "command": "add"
    }
  ]
}
```

### Performance Optimization
```csharp
// Prefer specific paths over broad selectors
// Good: specific path
.Add(SumBuilders.Sum("$.orders[*].amount"))

// Less efficient: broad selection then filtering
.Add(SumBuilders.Sum("$.orders[*].items[*].price"))
```

### Combining with Filters
```json
{
  "path": "$.activeOrdersTotal",
  "value": "=sum($.orders[?(@.status == 'active')].amount)",
  "command": "add"
}
```

## Advanced Patterns

### Conditional Summation
```json
{
  "path": "$.conditionalSum",
  "value": "=sum($.items[*][(@.category == 'electronics') ? @.price : 0])",
  "command": "add"
}
```

### Time-Series Aggregation
```csharp
var script = new JLioScript()
    .Add(SumBuilders.Sum("$.dailyData[*].sales"))
    .OnPath("$.monthlySales")
    .Add(SumBuilders.Sum("$.monthlyData[*].sales"))
    .OnPath("$.yearlySales");
```

### Weighted Summation
```json
{
  "path": "$.weightedTotal",
  "value": "=sum($.items[*].value * $.items[*].weight)",
  "command": "add"
}
```