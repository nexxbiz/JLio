# max Function

Find the maximum (largest) value from multiple inputs.

## Overview

The `max` function finds the largest value among multiple numeric inputs, supporting numbers, numeric strings, arrays, and JSONPath expressions. It's essential for finding peaks, limits, and identifying maximum values in datasets.

## Syntax

### Expression Format
```json
"=max(value1, value2, ...)"
"=max(array)"
"=max($.path[*])"
```

### Builder Pattern
```csharp
MaxBuilders.Max(arguments...)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| values | number/string/array | Yes (1+) | One or more numeric values, arrays, or JSONPath expressions to compare |

## Examples

### Basic Usage
```json
{
  "path": "$.maximum",
  "value": "=max(10, 5, 8, 15, 7)",
  "command": "add"
}
```

**Result**: `15`

### Array Processing
```json
{
  "path": "$.highestScore",
  "value": "=max([92, 85, 78, 96, 88])",
  "command": "add"
}
```

**Result**: `96`

### JSONPath Integration
```json
{
  "path": "$.highestPrice",
  "value": "=max($.products[*].price)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "products": [
    {"name": "Laptop", "price": 999.99},
    {"name": "Mouse", "price": 29.99},
    {"name": "Monitor", "price": 299.99}
  ]
}
```

**Result**: `999.99`

### Mixed Data Types
```json
{
  "path": "$.maxValue",
  "value": "=max(100, '125.5', 90, '110')",
  "command": "add"
}
```

**Result**: `125.5`

### Performance Analysis
```json
{
  "path": "$.performancePeaks",
  "value": {
    "maxSales": "=max($.monthlyData[*].sales)",
    "maxProfit": "=max($.monthlyData[*].profit)",
    "maxCustomers": "=max($.monthlyData[*].customerCount)"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Maximum
```csharp
var script = new JLioScript()
    .Add(MaxBuilders.Max("$.temperatures[*]"))
    .OnPath("$.maxTemperature");
```

### Multi-Metric Analysis
```csharp
var script = new JLioScript()
    .Add(MaxBuilders.Max("$.sales[*].amount"))
    .OnPath("$.peakSale")
    .Add(MaxBuilders.Max("$.traffic[*].visitors"))
    .OnPath("$.peakTraffic")
    .Add(MaxBuilders.Max("$.server[*].load"))
    .OnPath("$.peakLoad");
```

### Capacity Planning
```csharp
var script = new JLioScript()
    .Add(MaxBuilders.Max("$.usage[*].cpu"))
    .OnPath("$.maxCpuUsage")
    .Add(MaxBuilders.Max("$.usage[*].memory"))
    .OnPath("$.maxMemoryUsage")
    .Add(MaxBuilders.Max("$.usage[*].storage"))
    .OnPath("$.maxStorageUsage");
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float/Double**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts using InvariantCulture:
```json
"=max('10', '15.5', '8')"     // Result: 15.5
"=max('-5', '3', '-2')"       // Result: 3
```

### Arrays
Processes arrays recursively:
```json
// Flat array
"=max([5, 2, 8, 12, 9])"      // Result: 12

// Nested arrays (flattened)
"=max([[3, 7], [15, 9]])"     // Result: 15

// Mixed array
"=max([10, '15.5', 8])"       // Result: 15.5
```

### Null Handling
- **null values**: Typically ignored in comparison
- **Empty arrays**: May return null or error

## Mathematical Behavior

### Comparison Logic
Returns the numerically largest value among all inputs.

### Negative Numbers
Correctly handles negative values:
```json
"=max(-5, -2, -8, -1)"        // Result: -1
"=max(-3, 0, 5)"              // Result: 5
```

### Floating Point Precision
Maintains precision in comparisons:
```json
"=max(3.14159, 3.14160)"      // Result: 3.14160
```

## Error Handling

### Argument Validation
```json
// Correct usage
"=max(1, 2, 3)"      // ? Result: 3
"=max([1, 2, 3])"    // ? Result: 3
"=max($.values[*])"  // ? Finds maximum in array

// Errors
"=max()"             // ? No arguments provided
"=max('text')"       // ? Non-numeric string
"=max({})"           // ? Object not supported
```

### Error Messages
- **No arguments**: "max requires at least one argument"
- **Invalid types**: "max can only handle numeric values or arrays"
- **Parse failures**: Non-numeric strings result in errors

## Use Cases

### Business Analytics
1. **Sales Analysis**: Find peak sales, revenue, performance
2. **Market Research**: Identify maximum market share, prices
3. **Performance Tracking**: Monitor highest achievements
4. **Capacity Analysis**: Determine maximum capacity usage

### Financial Applications
1. **Risk Management**: Find maximum exposure, losses
2. **Investment Analysis**: Identify highest returns, gains
3. **Budget Planning**: Determine maximum allocations
4. **Cost Analysis**: Find highest costs or expenses

### System Monitoring
1. **Performance Metrics**: Track peak CPU, memory usage
2. **Load Testing**: Identify maximum throughput
3. **Error Analysis**: Find peak error rates
4. **Resource Planning**: Determine maximum resource needs

## Performance Considerations

- **Comparison Speed**: O(n) linear time for n values
- **Memory Usage**: Efficient single-pass algorithm
- **String Parsing**: Minimal overhead with InvariantCulture
- **Array Processing**: Efficient recursive traversal

## Integration Examples

### Sales Performance Dashboard
```json
{
  "path": "$.salesMetrics",
  "value": {
    "peakDailySales": "=max($.dailySales[*])",
    "topSalesperson": "=max($.salespeople[*].totalSales)",
    "highestOrderValue": "=max($.orders[*].total)",
    "bestConversionRate": "=max($.campaigns[*].conversionRate)"
  },
  "command": "add"
}
```

### Infrastructure Monitoring
```csharp
var script = new JLioScript()
    .Add(MaxBuilders.Max("$.servers[*].cpuUsage"))
    .OnPath("$.infrastructure.peakCpu")
    .Add(MaxBuilders.Max("$.servers[*].memoryUsage"))
    .OnPath("$.infrastructure.peakMemory")
    .Add(MaxBuilders.Max("$.servers[*].responseTime"))
    .OnPath("$.infrastructure.worstResponseTime");
```

### Quality Assurance Tracking
```json
[
  {
    "path": "$.qualityMetrics.bestScore",
    "value": "=max($.testResults[*].score)",
    "command": "add"
  },
  {
    "path": "$.qualityMetrics.longestUptime",
    "value": "=max($.systems[*].uptime)",
    "command": "add"
  },
  {
    "path": "$.qualityMetrics.isExcellent",
    "value": "=max($.testResults[*].score) >= $.excellenceThreshold",
    "command": "add"
  }
]
```

## Related Functions

- **[min](min.md)**: Find minimum value
- **[avg](avg.md)**: Calculate average value
- **[median](median.md)**: Find middle value
- **[count](count.md)**: Count elements
- **[sum](sum.md)**: Calculate total
- **[abs](abs.md)**: Absolute value (useful with max for ranges)

## Best Practices

### Data Validation
```csharp
// Ensure data exists before finding maximum
var script = new JLioScript()
    .Add(CountBuilders.Count("$.values[*]"))
    .OnPath("$.dataCount")
    .Add(MaxBuilders.Max("$.values[*]"))
    .OnPath("$.maximum")
    .ConditionalOn("$.dataCount > 0");
```

### Threshold Monitoring
```json
{
  "path": "$.alerts",
  "command": "ifElse",
  "condition": "=max($.servers[*].cpuUsage) > $.criticalThreshold",
  "then": [
    {
      "path": "$.criticalAlert",
      "value": true,
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.systemStatus",
      "value": "NORMAL",
      "command": "add"
    }
  ]
}
```

### Statistical Analysis
```csharp
var script = new JLioScript()
    .Add(MinBuilders.Min("$.data[*]"))
    .OnPath("$.stats.minimum")
    .Add(MaxBuilders.Max("$.data[*]"))
    .OnPath("$.stats.maximum")
    .Add(CalculateBuilders.Calculate("$.stats.maximum - $.stats.minimum"))
    .OnPath("$.stats.range");
```

## Advanced Usage Patterns

### Conditional Maximum
```json
{
  "path": "$.analysis",
  "value": {
    "maxActiveRevenue": "=max($.products[?(@.active == true)].revenue)",
    "maxHighRatedScore": "=max($.products[?(@.rating >= 4)].score)",
    "maxInStockPrice": "=max($.products[?(@.inventory > 0)].price)"
  },
  "command": "add"
}
```

### Peak Detection
```csharp
var script = new JLioScript()
    .Add(MaxBuilders.Max("$.timeSeries[*].value"))
    .OnPath("$.peaks.globalMax")
    .Add(AvgBuilders.Avg("$.timeSeries[*].value"))
    .OnPath("$.peaks.average")
    .Add(CalculateBuilders.Calculate("$.peaks.globalMax > $.peaks.average * 1.5"))
    .OnPath("$.peaks.isSignificantPeak");
```

### Performance Benchmarking
```json
{
  "path": "$.benchmarks",
  "value": {
    "bestThroughput": "=max($.tests[*].throughput)",
    "lowestLatency": "=min($.tests[*].latency)",
    "highestAccuracy": "=max($.tests[*].accuracy)",
    "performanceRatio": "=max($.tests[*].throughput) / min($.tests[*].latency)"
  },
  "command": "add"
}
```

### Alert Escalation System
```json
{
  "path": "$.alerting",
  "command": "forEach",
  "items": "$.systems[*]",
  "actions": [
    {
      "path": "@.maxErrorRate",
      "value": "=max(@.metrics[*].errorRate)",
      "command": "add"
    },
    {
      "path": "@.alertLevel",
      "command": "ifElse",
      "condition": "=@.maxErrorRate > $.thresholds.critical",
      "then": [
        {"path": "@.alert", "value": "CRITICAL", "command": "add"}
      ],
      "else": [
        {
          "command": "ifElse",
          "condition": "=@.maxErrorRate > $.thresholds.warning",
          "then": [
            {"path": "@.alert", "value": "WARNING", "command": "add"}
          ],
          "else": [
            {"path": "@.alert", "value": "NORMAL", "command": "add"}
          ]
        }
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
  "path": "$.safeMaximum",
  "command": "ifElse",
  "condition": "=count($.values[*]) > 0",
  "then": [
    {
      "path": "$.result",
      "value": "=max($.values[*])",
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

### Outlier Awareness
```csharp
// Consider outliers in maximum values
var script = new JLioScript()
    .Add(MaxBuilders.Max("$.values[*]"))
    .OnPath("$.maximum")
    .Add(AvgBuilders.Avg("$.values[*]"))
    .OnPath("$.average")
    .Add(CalculateBuilders.Calculate("$.maximum / $.average"))
    .OnPath("$.outlierRatio");  // High ratio indicates potential outlier
```

### Data Quality Checks
```json
{
  "path": "$.qualityCheck",
  "value": {
    "maxValue": "=max($.measurements[*])",
    "isWithinExpectedRange": "=max($.measurements[*]) <= $.expectedMax",
    "exceedsThreshold": "=max($.measurements[*]) > $.warningThreshold"
  },
  "command": "add"
}
```

## Comparison with Related Functions

### Max vs Min vs Avg
```json
{
  "path": "$.statistics",
  "value": {
    "minimum": "=min($.data[*])",      // Smallest value
    "maximum": "=max($.data[*])",      // Largest value  
    "average": "=avg($.data[*])",      // Central tendency
    "range": "=max($.data[*]) - min($.data[*])",  // Spread
    "midpoint": "=(max($.data[*]) + min($.data[*])) / 2"  // Range center
  },
  "command": "add"
}
```

### Peak vs Trend Analysis
```csharp
// Identify both peaks and trends
var script = new JLioScript()
    .Add(MaxBuilders.Max("$.recentData[*]"))
    .OnPath("$.currentPeak")
    .Add(MaxBuilders.Max("$.historicalData[*]"))
    .OnPath("$.historicalPeak")
    .Add(CalculateBuilders.Calculate("$.currentPeak > $.historicalPeak"))
    .OnPath("$.isNewRecord");
```