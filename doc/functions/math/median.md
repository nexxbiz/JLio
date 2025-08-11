# median Function

Calculate the median (middle value) of a numeric dataset.

## Overview

The `median` function finds the middle value in a sorted dataset, providing a measure of central tendency that's less sensitive to outliers than the mean. It supports arrays, numeric values, and JSONPath expressions, making it ideal for statistical analysis and data summarization.

## Syntax

### Expression Format
```json
"=median(array)"
"=median(value1, value2, ...)"
"=median($.path[*])"
```

### Builder Pattern
```csharp
MedianBuilders.Median(arguments...)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| values | number/string/array | Yes (1+) | Numeric values, arrays, or JSONPath expressions to find the median of |

## Examples

### Basic Median (Odd Count)
```json
{
  "path": "$.medianValue",
  "value": "=median([1, 3, 5, 7, 9])",
  "command": "add"
}
```

**Result**: `5` (middle value of 5 items)

### Basic Median (Even Count)
```json
{
  "path": "$.medianValue",
  "value": "=median([2, 4, 6, 8])",
  "command": "add"
}
```

**Result**: `5.0` (average of 4 and 6)

### JSONPath Integration
```json
{
  "path": "$.medianSalary",
  "value": "=median($.employees[*].salary)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "employees": [
    {"name": "Alice", "salary": 50000},
    {"name": "Bob", "salary": 60000},
    {"name": "Carol", "salary": 55000},
    {"name": "David", "salary": 75000},
    {"name": "Eve", "salary": 45000}
  ]
}
```

**Result**: `55000` (middle salary when sorted)

### Multiple Arguments
```json
{
  "path": "$.testMedian",
  "value": "=median(10, 20, 15, 30, 25)",
  "command": "add"
}
```

**Result**: `20` (middle value when sorted: 10, 15, 20, 25, 30)

### Statistical Analysis
```json
{
  "path": "$.statistics",
  "value": {
    "mean": "=avg($.scores[*])",
    "median": "=median($.scores[*])",
    "range": "=max($.scores[*]) - min($.scores[*])",
    "isSkewed": "=abs(avg($.scores[*]) - median($.scores[*])) > 5"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Median
```csharp
var script = new JLioScript()
    .Add(MedianBuilders.Median("$.values[*]"))
    .OnPath("$.medianValue");
```

### Comparative Analysis
```csharp
var script = new JLioScript()
    .Add(MedianBuilders.Median("$.currentPeriod[*]"))
    .OnPath("$.currentMedian")
    .Add(MedianBuilders.Median("$.previousPeriod[*]"))
    .OnPath("$.previousMedian")
    .Add(SubtractBuilders.Subtract("$.currentMedian", "$.previousMedian"))
    .OnPath("$.medianChange");
```

### Robust Statistics
```csharp
var script = new JLioScript()
    .Add(MedianBuilders.Median("$.data[*]"))
    .OnPath("$.stats.median")
    .Add(AvgBuilders.Avg("$.data[*]"))
    .OnPath("$.stats.mean")
    .Add(CalculateBuilders.Calculate("abs($.stats.mean - $.stats.median)"))
    .OnPath("$.stats.meanMedianDifference");
```

## Data Type Support

### Numeric Types
- **Integer**: `42`
- **Float/Double**: `3.14159`
- **Decimal**: `99.99`

### String Numbers
Automatically converts using InvariantCulture:
```json
"=median(['10', '5', '15'])"     // Result: 10 (sorted: 5, 10, 15)
"=median('3', '7', '1', '9')"    // Result: 5 (average of 3 and 7)
```

### Arrays
Processes arrays and flattens nested structures:
```json
// Flat array
"=median([3, 1, 4, 1, 5])"       // Result: 3 (sorted: 1, 1, 3, 4, 5)

// Nested arrays (flattened)
"=median([[1, 5], [3, 2]])"      // Result: 2.5 (sorted: 1, 2, 3, 5)

// Mixed types
"=median([10, '5', 15])"         // Result: 10
```

### Null Handling
- **null values**: Typically ignored in calculation
- **Empty arrays**: May return null or error

## Mathematical Behavior

### Median Calculation Rules
- **Odd count**: Middle value when sorted
- **Even count**: Average of two middle values when sorted
- **Single value**: Returns that value
- **Duplicate values**: Included in position calculations

### Examples
```json
// Odd count: [1, 3, 5] ? median = 3
"=median([5, 1, 3])"             // Result: 3

// Even count: [1, 2, 3, 4] ? median = (2 + 3) / 2 = 2.5
"=median([4, 1, 3, 2])"          // Result: 2.5

// With duplicates: [1, 2, 2, 3] ? median = (2 + 2) / 2 = 2
"=median([2, 1, 2, 3])"          // Result: 2
```

## Error Handling

### Argument Validation
```json
// Correct usage
"=median([1, 2, 3])"     // ? Result: 2
"=median(1, 2, 3)"       // ? Result: 2
"=median($.values[*])"   // ? Finds median of array

// Errors
"=median()"              // ? No arguments provided
"=median('text')"        // ? Non-numeric string
"=median({})"            // ? Object not supported
```

### Error Messages
- **No arguments**: "median requires at least one argument"
- **Invalid types**: "median can only handle numeric values or arrays"
- **Empty datasets**: May return null or appropriate error

## Use Cases

### Statistical Analysis
1. **Central Tendency**: Robust measure of center, less sensitive to outliers
2. **Data Distribution**: Understanding data spread and skewness
3. **Quality Control**: Median performance metrics
4. **Benchmarking**: Comparative analysis against median values

### Business Analytics
1. **Salary Analysis**: Median employee compensation
2. **Sales Performance**: Median sales figures
3. **Customer Analytics**: Median customer lifetime value
4. **Market Research**: Median price points, satisfaction scores

### Financial Applications
1. **Risk Analysis**: Median returns, losses
2. **Portfolio Analysis**: Median asset performance
3. **Credit Scoring**: Median credit scores by segment
4. **Real Estate**: Median home prices, rental rates

## Performance Considerations

- **Sorting Required**: O(n log n) time complexity due to sorting
- **Memory Usage**: Creates sorted copy of data
- **Large Datasets**: More memory intensive than mean calculation
- **String Conversion**: Minimal overhead with InvariantCulture

## Integration Examples

### Comprehensive Statistical Summary
```json
{
  "path": "$.statisticalSummary",
  "value": {
    "count": "=count($.data[*])",
    "minimum": "=min($.data[*])",
    "firstQuartile": "=median($.data[*][@ <= median($.data[*])])",
    "median": "=median($.data[*])",
    "thirdQuartile": "=median($.data[*][@ >= median($.data[*])])",
    "maximum": "=max($.data[*])",
    "mean": "=avg($.data[*])",
    "range": "=max($.data[*]) - min($.data[*])"
  },
  "command": "add"
}
```

### Salary Equity Analysis
```csharp
var script = new JLioScript()
    .Add(MedianBuilders.Median("$.employees[*].salary"))
    .OnPath("$.overallMedianSalary")
    .Add(MedianBuilders.Median("$.employees[?(@.department == 'Engineering')].salary"))
    .OnPath("$.engineeringMedianSalary")
    .Add(MedianBuilders.Median("$.employees[?(@.department == 'Sales')].salary"))
    .OnPath("$.salesMedianSalary")
    .Add(MedianBuilders.Median("$.employees[?(@.gender == 'Female')].salary"))
    .OnPath("$.femaleMedianSalary")
    .Add(MedianBuilders.Median("$.employees[?(@.gender == 'Male')].salary"))
    .OnPath("$.maleMedianSalary");
```

### Performance Monitoring
```json
[
  {
    "path": "$.performanceMetrics.medianResponseTime",
    "value": "=median($.requests[*].responseTime)",
    "command": "add"
  },
  {
    "path": "$.performanceMetrics.medianThroughput",
    "value": "=median($.servers[*].throughput)",
    "command": "add"
  },
  {
    "path": "$.performanceMetrics.medianErrorRate",
    "value": "=median($.servers[*].errorRate)",
    "command": "add"
  },
  {
    "path": "$.performanceMetrics.isPerformanceStable",
    "value": "=abs(avg($.requests[*].responseTime) - median($.requests[*].responseTime)) < 50",
    "command": "add"
  }
]
```

## Related Functions

- **[avg](avg.md)**: Calculate mean (alternative measure of central tendency)
- **[min](min.md)**: Find minimum value
- **[max](max.md)**: Find maximum value
- **[count](count.md)**: Count elements (needed for median calculation)
- **[sum](sum.md)**: Calculate total
- **[abs](abs.md)**: Absolute value (useful for comparing mean vs median)

## Statistical Properties

### Robustness to Outliers
Unlike the mean, median is robust to extreme values:
```json
{
  "path": "$.outlierComparison",
  "value": {
    "normalData": [1, 2, 3, 4, 5],
    "normalMean": "=avg([1, 2, 3, 4, 5])",      // Result: 3
    "normalMedian": "=median([1, 2, 3, 4, 5])",  // Result: 3
    
    "withOutlier": [1, 2, 3, 4, 100],
    "outlierMean": "=avg([1, 2, 3, 4, 100])",    // Result: 22 (heavily affected)
    "outlierMedian": "=median([1, 2, 3, 4, 100])" // Result: 3 (unchanged)
  },
  "command": "add"
}
```

### Distribution Analysis
```json
{
  "path": "$.distributionAnalysis",
  "value": {
    "mean": "=avg($.data[*])",
    "median": "=median($.data[*])",
    "skewness": "=avg($.data[*]) - median($.data[*])",
    "isRightSkewed": "=avg($.data[*]) > median($.data[*])",
    "isLeftSkewed": "=avg($.data[*]) < median($.data[*])",
    "isSymmetric": "=abs(avg($.data[*]) - median($.data[*])) < 0.1"
  },
  "command": "add"
}
```

## Best Practices

### Data Quality Validation
```csharp
// Ensure adequate sample size
var script = new JLioScript()
    .Add(CountBuilders.Count("$.data[*]"))
    .OnPath("$.sampleSize")
    .Add(MedianBuilders.Median("$.data[*]"))
    .OnPath("$.median")
    .ConditionalOn("$.sampleSize >= 3");
```

### Comparison with Mean
```json
{
  "path": "$.robustStatistics",
  "command": "ifElse",
  "condition": "=count($.data[*]) > 10",
  "then": [
    {
      "path": "$.centralTendency",
      "value": {
        "mean": "=avg($.data[*])",
        "median": "=median($.data[*])",
        "recommendedMeasure": "=abs(avg($.data[*]) - median($.data[*])) > avg($.data[*]) * 0.1 ? 'median' : 'mean'"
      },
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.warning",
      "value": "Sample size too small for reliable median",
      "command": "add"
    }
  ]
}
```

### Performance Optimization
```csharp
// For large datasets, consider if median is necessary
var script = new JLioScript()
    .Add(CountBuilders.Count("$.largeDataset[*]"))
    .OnPath("$.datasetSize")
    .Add(MedianBuilders.Median("$.largeDataset[*]"))
    .OnPath("$.median")
    .ConditionalOn("$.datasetSize <= 10000");  // Limit for performance
```

## Advanced Usage Patterns

### Quartile Analysis
```json
{
  "path": "$.quartileAnalysis",
  "value": {
    "q1": "=median($.data[*][@ < median($.data[*])])",
    "q2": "=median($.data[*])",
    "q3": "=median($.data[*][@ > median($.data[*])])",
    "iqr": "=median($.data[*][@ > median($.data[*])]) - median($.data[*][@ < median($.data[*])])"
  },
  "command": "add"
}
```

### Conditional Medians
```json
{
  "path": "$.segmentAnalysis",
  "value": {
    "highPerformers": "=median($.employees[?(@.performance >= 4)].salary)",
    "averagePerformers": "=median($.employees[?(@.performance >= 3 && @.performance < 4)].salary)",
    "lowPerformers": "=median($.employees[?(@.performance < 3)].salary)"
  },
  "command": "add"
}
```

### Time Series Median Analysis
```csharp
var script = new JLioScript()
    .Add(MedianBuilders.Median("$.dailyValues[-30:]"))  // Last 30 days
    .OnPath("$.monthlyMedian")
    .Add(MedianBuilders.Median("$.dailyValues[-7:]"))   // Last 7 days
    .OnPath("$.weeklyMedian")
    .Add(CalculateBuilders.Calculate("$.weeklyMedian / $.monthlyMedian"))
    .OnPath("$.recentTrendRatio");
```

## Common Pitfalls

### Empty Dataset Handling
```json
// Always validate data exists
{
  "path": "$.safeMedian",
  "command": "ifElse",
  "condition": "=count($.values[*]) > 0",
  "then": [
    {
      "path": "$.result",
      "value": "=median($.values[*])",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.result",
      "value": "No data available",
      "command": "add"
    }
  ]
}
```

### Performance with Large Datasets
```csharp
// Consider sampling for very large datasets
var script = new JLioScript()
    .Add(CountBuilders.Count("$.hugeDataset[*]"))
    .OnPath("$.dataSize")
    .Add(MedianBuilders.Median("$.hugeDataset[*]"))
    .OnPath("$.exactMedian")
    .ConditionalOn("$.dataSize <= 50000");
```

### Understanding Even Count Behavior
```json
{
  "path": "$.medianExamples",
  "value": {
    "oddCount": "=median([1, 2, 3])",         // Result: 2 (middle value)
    "evenCount": "=median([1, 2, 3, 4])",     // Result: 2.5 (average of 2 and 3)
    "evenCountSame": "=median([1, 2, 2, 3])", // Result: 2 (average of 2 and 2)
    "singleValue": "=median([5])"             // Result: 5
  },
  "command": "add"
}
```