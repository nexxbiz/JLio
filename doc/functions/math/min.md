# min Function

Find the minimum (smallest) value from multiple inputs.

## Overview

The `min` function finds the smallest value among multiple numeric inputs, supporting numbers, numeric strings, arrays, and JSONPath expressions. It's essential for finding limits, boundaries, and identifying minimum values in datasets.

## Syntax

### Expression Format
```json
"=min(value1, value2, ...)"
"=min(array)"
"=min($.path[*])"
```

### Builder Pattern
```csharp
MinBuilders.Min(arguments...)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| values | number/string/array | Yes (1+) | One or more numeric values, arrays, or JSONPath expressions to compare |

## Examples

### Basic Usage
```json
{
  "path": "$.minimum",
  "value": "=min(10, 5, 8, 3, 7)",
  "command": "add"
}
```

**Result**: `3`

### Array Processing
```json
{
  "path": "$.lowestScore",
  "value": "=min([92, 85, 78, 96, 88])",
  "command": "add"
}
```

**Result**: `78`

### JSONPath Integration
```json
{
  "path": "$.lowestPrice",
  "value": "=min($.products[*].price)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "products": [
    {"name": "Laptop", "price": 999.99},
    {"name": "Mouse", "price": 29.99},
    {"name": "Keyboard", "price": 79.99}
  ]
}
```

**Result**: `29.99`

### Mixed Data Types
```json
{
  "path": "$.minValue",
  "value": "=min(100, '85.5', 120, '90')",
  "command": "add"
}
```

**Result**: `85.5`

### Business Applications
```json
{
  "path": "$.analysis",
  "value": {
    "minOrderAmount": "=min($.orders[*].total)",
    "minCustomerAge": "=min($.customers[*].age)",
    "minInventoryLevel": "=min($.inventory[*].quantity)"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Minimum
```csharp
var script = new JLioScript()
    .Add(MinBuilders.Min("$.temperatures[*]"))
    .OnPath("$.minTemperature");
```

### Multi-Dataset Analysis
```csharp
var script = new JLioScript()
    .Add(MinBuilders.Min("$.sales[*].amount"))
    .OnPath("$.minSale")
    .Add(MinBuilders.Min("$.expenses[*].amount"))
    .OnPath("$.minExpense")
    .Add(MinBuilders.Min("$.profits[*]"))
    .OnPath("$.minProfit");
```

### Performance Monitoring
```csharp
var script = new JLioScript()
    .Add(MinBuilders.Min("$.servers[*].responseTime"))
    .OnPath("$.bestResponseTime")
    .Add(MinBuilders.Min("$.servers[*].cpuUsage"))
    .OnPath("$.minCpuUsage");
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float/Double**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts using InvariantCulture:
```json
"=min('10', '5.5', '8')"      // Result: 5.5
"=min('-5', '3', '-2')"       // Result: -5
```

### Arrays
Processes arrays recursively:
```json
// Flat array
"=min([5, 2, 8, 1, 9])"       // Result: 1

// Nested arrays (flattened)
"=min([[3, 7], [1, 9]])"      // Result: 1

// Mixed array
"=min([10, '5.5', 8])"        // Result: 5.5
```

### Null Handling
- **null values**: Typically ignored in comparison
- **Empty arrays**: May return null or error

## Mathematical Behavior

### Comparison Logic
Returns the numerically smallest value among all inputs.

### Negative Numbers
Correctly handles negative values:
```json
"=min(-5, -2, -8, -1)"        // Result: -8
"=min(-3, 0, 5)"              // Result: -3
```

### Floating Point Precision
Maintains precision in comparisons:
```json
"=min(3.14159, 3.14158)"      // Result: 3.14158
```

## Error Handling

### Argument Validation
```json
// Correct usage
"=min(1, 2, 3)"      // ? Result: 1
"=min([1, 2, 3])"    // ? Result: 1
"=min($.values[*])"  // ? Finds minimum in array

// Errors
"=min()"             // ? No arguments provided
"=min('text')"       // ? Non-numeric string
"=min({})"           // ? Object not supported
```

### Error Messages
- **No arguments**: "min requires at least one argument"
- **Invalid types**: "min can only handle numeric values or arrays"
- **Parse failures**: Non-numeric strings result in errors

## Use Cases

### Business Analytics
1. **Price Analysis**: Find minimum prices, costs, fees
2. **Performance Metrics**: Identify best performance values
3. **Quality Control**: Find minimum acceptable thresholds
4. **Resource Management**: Identify minimum resource requirements

### Financial Applications
1. **Budget Analysis**: Find minimum budget allocations
2. **Cost Optimization**: Identify lowest costs or expenses
3. **Risk Assessment**: Find minimum risk scenarios
4. **Investment Analysis**: Identify minimum returns

### Operations Management
1. **Inventory Control**: Monitor minimum stock levels
2. **Quality Assurance**: Track minimum quality scores
3. **Capacity Planning**: Identify minimum capacity requirements
4. **Performance Monitoring**: Find best response times

## Performance Considerations

- **Comparison Speed**: O(n) linear time for n values
- **Memory Usage**: Efficient single-pass algorithm
- **String Parsing**: Minimal overhead with InvariantCulture
- **Array Processing**: Efficient recursive traversal

## Integration Examples

### Supply Chain Analytics
```json
{
  "path": "$.supplyChainMetrics",
  "value": {
    "minLeadTime": "=min($.suppliers[*].leadTime)",
    "minPrice": "=min($.suppliers[*].price)",
    "minQuality": "=min($.suppliers[*].qualityRating)",
    "minCapacity": "=min($.suppliers[*].capacity)"
  },
  "command": "add"
}
```

### System Performance Dashboard
```csharp
var script = new JLioScript()
    .Add(MinBuilders.Min("$.servers[*].responseTime"))
    .OnPath("$.performance.bestResponseTime")
    .Add(MinBuilders.Min("$.servers[*].errorRate"))
    .OnPath("$.performance.bestErrorRate")
    .Add(MinBuilders.Min("$.servers[*].downtime"))
    .OnPath("$.performance.minDowntime");
```

### Quality Control Analysis
```json
[
  {
    "path": "$.qualityMetrics.minDefectRate",
    "value": "=min($.batches[*].defectRate)",
    "command": "add"
  },
  {
    "path": "$.qualityMetrics.minComplianceScore",
    "value": "=min($.batches[*].complianceScore)",
    "command": "add"
  },
  {
    "path": "$.qualityMetrics.isWithinThreshold",
    "value": "=min($.batches[*].defectRate) < $.threshold",
    "command": "add"
  }
]
```

## Related Functions

- **[max](max.md)**: Find maximum value
- **[avg](avg.md)**: Calculate average value
- **[median](median.md)**: Find middle value
- **[count](count.md)**: Count elements
- **[sum](sum.md)**: Calculate total
- **[abs](abs.md)**: Absolute value (useful with min for distances)

## Best Practices

### Data Validation
```csharp
// Ensure data exists before finding minimum
var script = new JLioScript()
    .Add(CountBuilders.Count("$.values[*]"))
    .OnPath("$.dataCount")
    .Add(MinBuilders.Min("$.values[*]"))
    .OnPath("$.minimum")
    .ConditionalOn("$.dataCount > 0");
```

### Threshold Checking
```json
{
  "path": "$.alerts",
  "command": "ifElse",
  "condition": "=min($.inventory[*].quantity) < $.reorderThreshold",
  "then": [
    {
      "path": "$.lowInventoryAlert",
      "value": true,
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.inventoryStatus",
      "value": "OK",
      "command": "add"
    }
  ]
}
```

### Combining with Other Functions
```csharp
var script = new JLioScript()
    .Add(MinBuilders.Min("$.scores[*]"))
    .OnPath("$.stats.minimum")
    .Add(MaxBuilders.Max("$.scores[*]"))
    .OnPath("$.stats.maximum")
    .Add(AvgBuilders.Avg("$.scores[*]"))
    .OnPath("$.stats.average");
```

## Advanced Usage Patterns

### Conditional Minimum
```json
{
  "path": "$.analysis",
  "value": {
    "minActivePrice": "=min($.products[?(@.active == true)].price)",
    "minHighRatedPrice": "=min($.products[?(@.rating >= 4)].price)",
    "minInStockPrice": "=min($.products[?(@.inventory > 0)].price)"
  },
  "command": "add"
}
```

### Range Analysis
```csharp
var script = new JLioScript()
    .Add(MinBuilders.Min("$.measurements[*]"))
    .OnPath("$.range.minimum")
    .Add(MaxBuilders.Max("$.measurements[*]"))
    .OnPath("$.range.maximum")
    .Add(CalculateBuilders.Calculate("$.range.maximum - $.range.minimum"))
    .OnPath("$.range.span");
```

### Multi-Dimensional Analysis
```json
{
  "path": "$.optimization",
  "value": {
    "minCostOption": "=min($.options[*].cost)",
    "minTimeOption": "=min($.options[*].time)",
    "minRiskOption": "=min($.options[*].risk)",
    "bestOverallIndex": "=min($.options[*].compositeScore)"
  },
  "command": "add"
}
```

### Alert System Integration
```json
{
  "path": "$.monitoring",
  "command": "forEach",
  "items": "$.systems[*]",
  "actions": [
    {
      "path": "@.minResponseTime",
      "value": "=min(@.metrics[*].responseTime)",
      "command": "add"
    },
    {
      "path": "@.alertLevel",
      "command": "ifElse",
      "condition": "=@.minResponseTime > $.thresholds.responseTime",
      "then": [
        {"path": "@.alert", "value": "HIGH", "command": "add"}
      ],
      "else": [
        {"path": "@.alert", "value": "NORMAL", "command": "add"}
      ]
    }
  ]
}
```

## Common Pitfalls

### Empty Dataset Handling
```json
// Handle empty arrays gracefully
{
  "path": "$.safeMinimum",
  "command": "ifElse",
  "condition": "=count($.values[*]) > 0",
  "then": [
    {
      "path": "$.result",
      "value": "=min($.values[*])",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result",
      "value": null,
      "command": "add"
    }
  ]
}
```

### Type Consistency
```csharp
// Ensure consistent numeric types
var script = new JLioScript()
    .Add(MinBuilders.Min("$.numericValues[*][@ != null]"))  // Filter nulls
    .OnPath("$.minimum");
```

### Precision Considerations
```json
{
  "path": "$.preciseMinimum",
  "value": "=round(min($.measurements[*]), 4)",
  "command": "add"
}
```