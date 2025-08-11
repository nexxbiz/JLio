# avg Function

Calculate the arithmetic mean (average) of numeric values.

## Overview

The `avg` function calculates the arithmetic mean of multiple numeric values, supporting numbers, numeric strings, arrays, and JSONPath expressions. It automatically handles type conversion and can process nested arrays, making it ideal for statistical analysis and data summarization.

## Syntax

### Expression Format
```json
"=avg(value1, value2, ...)"
"=avg(array)"
"=avg($.path[*])"
```

### Builder Pattern
```csharp
AvgBuilders.Avg(arguments...)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| values | number/string/array | Yes (1+) | One or more numeric values, arrays, or JSONPath expressions to average |

## Examples

### Basic Usage
```json
{
  "path": "$.average",
  "value": "=avg(10, 20, 30)",
  "command": "add"
}
```

**Result**: `20`

### Array Processing
```json
{
  "path": "$.arrayAverage",
  "value": "=avg([85, 92, 78, 96, 88])",
  "command": "add"
}
```

**Result**: `87.8`

### JSONPath Integration
```json
{
  "path": "$.averageScore",
  "value": "=avg($.students[*].score)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "students": [
    {"name": "Alice", "score": 92},
    {"name": "Bob", "score": 85},
    {"name": "Carol", "score": 90}
  ]
}
```

**Result**: `89`

### String Number Support
```json
{
  "path": "$.mixedAverage",
  "value": "=avg(100, '85.5', 90)",
  "command": "add"
}
```

**Result**: `91.83` (approximately)

### Performance Metrics
```json
{
  "path": "$.performanceMetrics",
  "value": {
    "avgResponseTime": "=avg($.requests[*].responseTime)",
    "avgThroughput": "=avg($.servers[*].throughput)",
    "avgCpuUsage": "=avg($.servers[*].cpuUsage)"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Average
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.prices[*]"))
    .OnPath("$.averagePrice");
```

### Multi-Metric Analysis
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.sales[*].amount"))
    .OnPath("$.averageSales")
    .Add(AvgBuilders.Avg("$.sales[*].quantity"))
    .OnPath("$.averageQuantity")
    .Add(AvgBuilders.Avg("$.customers[*].age"))
    .OnPath("$.averageCustomerAge");
```

### Statistical Dashboard
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.measurements[*]"))
    .OnPath("$.statistics.mean")
    .Add(MinBuilders.Min("$.measurements[*]"))
    .OnPath("$.statistics.minimum")
    .Add(MaxBuilders.Max("$.measurements[*]"))
    .OnPath("$.statistics.maximum");
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float/Double**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts using InvariantCulture:
```json
"=avg('10', '20.5', '30')"    // Result: 20.17 (approximately)
"=avg('-5', '3', '12')"       // Result: 3.33 (approximately)
```

### Arrays
Processes arrays recursively:
```json
// Flat array
"=avg([1, 2, 3, 4, 5])"       // Result: 3

// Nested arrays (flattened automatically)
"=avg([[1, 2], [3, 4]])"      // Result: 2.5

// Mixed types in array
"=avg([1, '2.5', 3])"         // Result: 2.17 (approximately)
```

### Null Handling
- **null values**: Ignored in calculation (not counted in divisor)
- **Empty arrays**: Results in 0

## Mathematical Behavior

### Standard Arithmetic Mean
Formula: `avg(x?, x?, ..., x?) = (x? + x? + ... + x?) / n`

### Examples
```json
"=avg(2, 4, 6)"               // Result: 4 ((2+4+6)/3)
"=avg(10, 30)"                // Result: 20 ((10+30)/2)
"=avg(1, 2, 3, 4, 5)"         // Result: 3 ((1+2+3+4+5)/5)
```

### Precision Considerations
Results may have many decimal places:
```json
"=avg(1, 2, 4)"               // Result: 2.333333... 
"=round(avg(1, 2, 4), 2)"     // Result: 2.33 (rounded)
```

## Error Handling

### Argument Validation
```json
// Correct usage
"=avg(1, 2, 3)"      // ? Result: 2
"=avg([1, 2, 3])"    // ? Result: 2
"=avg($.values[*])"  // ? Averages array values

// Errors
"=avg()"             // ? No arguments provided
"=avg('text')"       // ? Non-numeric string
"=avg({})"           // ? Object not supported
```

### Error Messages
- **No arguments**: "avg requires at least one argument"
- **Invalid types**: "avg can only handle numeric values or arrays"
- **Empty datasets**: May return 0 or null depending on implementation

## Use Cases

### Business Analytics
1. **Sales Performance**: Average sales amounts, quantities, conversion rates
2. **Customer Analytics**: Average customer age, spending, satisfaction scores
3. **Financial Analysis**: Average revenue, expenses, profit margins
4. **Quality Metrics**: Average defect rates, completion times

### Educational Applications
1. **Grade Calculations**: Student average scores, class averages
2. **Assessment Analytics**: Average test scores, performance trends
3. **Progress Tracking**: Average improvement rates
4. **Comparative Analysis**: Average performance across groups

### Performance Monitoring
1. **System Metrics**: Average response times, CPU usage, memory consumption
2. **Application Performance**: Average request processing times
3. **Network Analytics**: Average bandwidth usage, latency
4. **User Experience**: Average page load times, session duration

## Performance Considerations

- **Calculation Complexity**: O(n) linear time for n values
- **Memory Usage**: Efficient streaming for large datasets
- **Precision**: Uses double-precision floating point
- **Array Processing**: Recursive processing with efficient traversal

## Integration Examples

### Student Performance Dashboard
```json
{
  "path": "$.classAnalytics",
  "value": {
    "averageScore": "=avg($.students[*].finalGrade)",
    "averageAttendance": "=avg($.students[*].attendanceRate)",
    "averageAssignments": "=avg($.students[*].assignmentsCompleted)",
    "classSize": "=count($.students[*])"
  },
  "command": "add"
}
```

### Sales Performance Analysis
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.sales[*].amount"))
    .OnPath("$.metrics.avgSaleAmount")
    .Add(AvgBuilders.Avg("$.sales[*].quantity"))
    .OnPath("$.metrics.avgQuantity")
    .Add(AvgBuilders.Avg("$.salespeople[*].totalSales"))
    .OnPath("$.metrics.avgSalespersonPerformance");
```

### Server Performance Monitoring
```json
[
  {
    "path": "$.monitoring.avgCpuUsage",
    "value": "=avg($.servers[*].cpu.usage)",
    "command": "add"
  },
  {
    "path": "$.monitoring.avgMemoryUsage",
    "value": "=avg($.servers[*].memory.usage)",
    "command": "add"
  },
  {
    "path": "$.monitoring.avgResponseTime", 
    "value": "=avg($.servers[*].responseTime)",
    "command": "add"
  }
]
```

## Related Functions

- **[sum](sum.md)**: Calculate total (avg = sum / count)
- **[count](count.md)**: Count elements (used in average calculation)
- **[min](min.md)**: Find minimum value
- **[max](max.md)**: Find maximum value
- **[median](median.md)**: Find middle value (alternative to mean)
- **[round](round.md)**: Round average results for display

## Statistical Properties

### Central Tendency
The average represents the central tendency of a dataset, showing the typical or expected value.

### Relationship to Other Measures
```json
{
  "path": "$.statistics",
  "value": {
    "mean": "=avg($.data[*])",
    "median": "=median($.data[*])", 
    "range": "=max($.data[*]) - min($.data[*])",
    "count": "=count($.data[*])"
  },
  "command": "add"
}
```

### Sensitivity to Outliers
Averages are sensitive to extreme values. Consider using median for skewed distributions.

## Best Practices

### Round Results for Display
```csharp
// Good: Round averages for presentation
var script = new JLioScript()
    .Add(RoundBuilders.Round("avg($.values[*])", "2"))
    .OnPath("$.displayAverage");
```

### Validate Data Quality
```json
{
  "path": "$.qualityAverage",
  "command": "ifElse",
  "condition": "=count($.values[*]) > 0",
  "then": [
    {
      "path": "$.result",
      "value": "=avg($.values[?(@)])", // Filter out nulls
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

### Combine with Other Statistics
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.scores[*]"))
    .OnPath("$.stats.average")
    .Add(MinBuilders.Min("$.scores[*]"))
    .OnPath("$.stats.minimum")
    .Add(MaxBuilders.Max("$.scores[*]"))
    .OnPath("$.stats.maximum")
    .Add(CountBuilders.Count("$.scores[*]"))
    .OnPath("$.stats.sampleSize");
```

## Advanced Usage Patterns

### Weighted Average Simulation
```json
{
  "path": "$.weightedAverage",
  "value": "=sum($.items[*].value * $.items[*].weight) / sum($.items[*].weight)",
  "command": "add"
}
```

### Rolling Average Calculation
```csharp
// Calculate moving average over time periods
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.timeSeries[-7:]"))  // Last 7 periods
    .OnPath("$.sevenDayAverage")
    .Add(AvgBuilders.Avg("$.timeSeries[-30:]")) // Last 30 periods
    .OnPath("$.thirtyDayAverage");
```

### Comparative Analysis
```json
{
  "path": "$.comparison",
  "value": {
    "currentPeriodAvg": "=avg($.currentPeriod[*].value)",
    "previousPeriodAvg": "=avg($.previousPeriod[*].value)",
    "percentChange": "=(avg($.currentPeriod[*].value) - avg($.previousPeriod[*].value)) / avg($.previousPeriod[*].value) * 100"
  },
  "command": "add"
}
```

### Conditional Averaging
```json
{
  "path": "$.conditionalAverages",
  "value": {
    "highPerformers": "=avg($.employees[?(@.performance > 4)].salary)",
    "averagePerformers": "=avg($.employees[?(@.performance >= 3 && @.performance <= 4)].salary)",
    "needsImprovement": "=avg($.employees[?(@.performance < 3)].salary)"
  },
  "command": "add"
}
```

## Common Pitfalls

### Empty Datasets
```json
// Handle empty arrays gracefully  
{
  "path": "$.safeAverage",
  "value": "=count($.values[*]) > 0 ? avg($.values[*]) : 0",
  "command": "add"
}
```

### Outlier Sensitivity
```csharp
// Consider median for skewed data
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.salaries[*]"))
    .OnPath("$.meanSalary")
    .Add(MedianBuilders.Median("$.salaries[*]"))
    .OnPath("$.medianSalary");  // Often more representative
```

### Precision Issues
```json
// Round averages appropriately
{
  "path": "$.displayValues",
  "value": {
    "exactAverage": "=avg($.values[*])",
    "displayAverage": "=round(avg($.values[*]), 2)"
  },
  "command": "add"
}
```