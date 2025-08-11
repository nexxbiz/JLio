# count Function

Count the number of elements in arrays or objects.

## Overview

The `count` function returns the number of elements in arrays, objects, or collections accessed via JSONPath expressions. It's essential for statistical analysis, data validation, and understanding dataset sizes.

## Syntax

### Expression Format
```json
"=count(array)"
"=count($.path[*])"
"=count(object)"
```

### Builder Pattern
```csharp
CountBuilders.Count(argument)
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| collection | array/object/path | Yes | Array, object, or JSONPath expression to count |

## Examples

### Basic Array Counting
```json
{
  "path": "$.itemCount",
  "value": "=count([1, 2, 3, 4, 5])",
  "command": "add"
}
```

**Result**: `5`

### JSONPath Array Counting
```json
{
  "path": "$.studentCount",
  "value": "=count($.students[*])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "students": [
    {"name": "Alice", "grade": 92},
    {"name": "Bob", "grade": 85},
    {"name": "Carol", "grade": 90}
  ]
}
```

**Result**: `3`

### Object Property Counting
```json
{
  "path": "$.propertyCount",
  "value": "=count($.configuration)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "configuration": {
    "host": "localhost",
    "port": 8080,
    "timeout": 30,
    "debug": true
  }
}
```

**Result**: `4`

### Filtered Counting
```json
{
  "path": "$.activeUserCount",
  "value": "=count($.users[?(@.active == true)])",
  "command": "add"
}
```

### Business Analytics
```json
{
  "path": "$.businessMetrics",
  "value": {
    "totalOrders": "=count($.orders[*])",
    "activeCustomers": "=count($.customers[?(@.status == 'active')])",
    "completedTasks": "=count($.tasks[?(@.completed == true)])",
    "availableProducts": "=count($.products[?(@.inventory > 0)])"
  },
  "command": "add"
}
```

## Builder Pattern Usage

### Simple Counting
```csharp
var script = new JLioScript()
    .Add(CountBuilders.Count("$.items[*]"))
    .OnPath("$.itemCount");
```

### Multi-Dataset Analysis
```csharp
var script = new JLioScript()
    .Add(CountBuilders.Count("$.sales[*]"))
    .OnPath("$.totalSales")
    .Add(CountBuilders.Count("$.customers[*]"))
    .OnPath("$.totalCustomers")
    .Add(CountBuilders.Count("$.products[*]"))
    .OnPath("$.totalProducts");
```

### Statistical Foundation
```csharp
var script = new JLioScript()
    .Add(CountBuilders.Count("$.data[*]"))
    .OnPath("$.sampleSize")
    .Add(SumBuilders.Sum("$.data[*]"))
    .OnPath("$.total")
    .Add(CalculateBuilders.Calculate("$.total / $.sampleSize"))
    .OnPath("$.average");
```

## Data Type Support

### Arrays
Counts array elements including nested structures:
```json
"=count([1, 2, 3])"           // Result: 3
"=count([[1, 2], [3, 4]])"    // Result: 2 (counts subarrays)
"=count([1, null, 3])"        // Result: 3 (includes null)
"=count([])"                  // Result: 0 (empty array)
```

### Objects
Counts object properties:
```json
"=count({a: 1, b: 2, c: 3})"  // Result: 3
"=count({})"                  // Result: 0 (empty object)
```

### JSONPath Expressions
Counts elements selected by JSONPath:
```json
"=count($.items[*])"          // Counts all items
"=count($.items[?(@.active)])" // Counts filtered items
"=count($.nested[*].values[*])" // Counts nested elements
```

## Mathematical Behavior

### Non-Negative Results
Count always returns 0 or positive integers:
- Minimum value: `0` (empty collections)
- Always integer results
- No negative values possible

### Null and Undefined Handling
- **null elements**: Counted as elements
- **undefined properties**: Not counted
- **null collections**: May return 0 or error

## Error Handling

### Argument Validation
```json
// Correct usage
"=count([1, 2, 3])"    // ? Result: 3
"=count($.items[*])"   // ? Counts array elements
"=count($.config)"     // ? Counts object properties

// Potential issues
"=count()"             // ? Missing required argument
"=count(42)"           // ? Cannot count scalar values
"=count('string')"     // ? Cannot count strings
```

### Error Messages
- **Missing arguments**: "count requires exactly one argument"
- **Invalid types**: "count can only handle arrays, objects, or collections"
- **Path not found**: May return 0 for non-existent paths

## Use Cases

### Data Validation
1. **Dataset Size Validation**: Ensure minimum/maximum dataset sizes
2. **Completeness Checks**: Verify all required data is present
3. **Quality Assurance**: Count valid vs invalid records
4. **Threshold Monitoring**: Alert when counts exceed limits

### Business Intelligence
1. **KPI Tracking**: Count customers, orders, transactions
2. **Performance Metrics**: Count successful operations, errors
3. **Capacity Planning**: Count current vs maximum capacity
4. **Trend Analysis**: Track count changes over time

### System Monitoring
1. **Resource Monitoring**: Count active connections, processes
2. **Error Tracking**: Count error occurrences
3. **Usage Analytics**: Count user actions, requests
4. **Health Checks**: Count healthy vs unhealthy components

## Performance Considerations

- **Execution Speed**: O(n) for arrays, O(1) for stored counts
- **Memory Usage**: Minimal memory overhead
- **JSONPath Processing**: Performance depends on path complexity
- **Large Collections**: Efficient for counting large datasets

## Integration Examples

### E-commerce Analytics Dashboard
```json
{
  "path": "$.dashboard",
  "value": {
    "totalProducts": "=count($.products[*])",
    "inStockProducts": "=count($.products[?(@.inventory > 0)])",
    "totalOrders": "=count($.orders[*])",
    "completedOrders": "=count($.orders[?(@.status == 'completed')])",
    "activeCustomers": "=count($.customers[?(@.lastOrderDate > $.thirtyDaysAgo)])",
    "inventoryTurnover": "=count($.products[?(@.inventory == 0)]) / count($.products[*]) * 100"
  },
  "command": "add"
}
```

### System Health Monitoring
```csharp
var script = new JLioScript()
    .Add(CountBuilders.Count("$.servers[*]"))
    .OnPath("$.monitoring.totalServers")
    .Add(CountBuilders.Count("$.servers[?(@.status == 'healthy')]"))
    .OnPath("$.monitoring.healthyServers")
    .Add(CountBuilders.Count("$.alerts[*]"))
    .OnPath("$.monitoring.totalAlerts")
    .Add(CountBuilders.Count("$.alerts[?(@.severity == 'critical')]"))
    .OnPath("$.monitoring.criticalAlerts");
```

### Survey Response Analysis
```json
[
  {
    "path": "$.survey.totalResponses",
    "value": "=count($.responses[*])",
    "command": "add"
  },
  {
    "path": "$.survey.completeResponses",
    "value": "=count($.responses[?(@.completed == true)])",
    "command": "add"
  },
  {
    "path": "$.survey.highSatisfaction",
    "value": "=count($.responses[?(@.satisfaction >= 4)])",
    "command": "add"
  },
  {
    "path": "$.survey.completionRate",
    "value": "=count($.responses[?(@.completed == true)]) / count($.responses[*]) * 100",
    "command": "add"
  }
]
```

## Related Functions

- **[sum](sum.md)**: Calculate totals (often used with count for averages)
- **[avg](avg.md)**: Calculate averages (uses count in calculation)
- **[min](min.md)**: Find minimum (count helps validate data existence)
- **[max](max.md)**: Find maximum (count helps validate data existence)
- **[median](median.md)**: Find median (requires count for calculation)

## Statistical Applications

### Descriptive Statistics
```csharp
var script = new JLioScript()
    .Add(CountBuilders.Count("$.data[*]"))
    .OnPath("$.stats.sampleSize")
    .Add(SumBuilders.Sum("$.data[*]"))
    .OnPath("$.stats.sum")
    .Add(CalculateBuilders.Calculate("$.stats.sum / $.stats.sampleSize"))
    .OnPath("$.stats.mean")
    .Add(MinBuilders.Min("$.data[*]"))
    .OnPath("$.stats.minimum")
    .Add(MaxBuilders.Max("$.data[*]"))
    .OnPath("$.stats.maximum");
```

### Data Quality Assessment
```json
{
  "path": "$.dataQuality",
  "value": {
    "totalRecords": "=count($.records[*])",
    "validRecords": "=count($.records[?(@.isValid == true)])",
    "nullValues": "=count($.records[*][@ == null])",
    "duplicates": "=count($.records[*]) - count($.uniqueRecords[*])",
    "qualityScore": "=count($.records[?(@.isValid == true)]) / count($.records[*]) * 100"
  },
  "command": "add"
}
```

## Best Practices

### Data Validation Before Operations
```csharp
// Validate data exists before processing
var script = new JLioScript()
    .Add(CountBuilders.Count("$.data[*]"))
    .OnPath("$.dataCount")
    .Add(AvgBuilders.Avg("$.data[*]"))
    .OnPath("$.average")
    .ConditionalOn("$.dataCount > 0");
```

### Combine with Conditional Logic
```json
{
  "path": "$.status",
  "command": "ifElse",
  "condition": "=count($.errors[*]) > 0",
  "then": [
    {
      "path": "$.systemHealth",
      "value": "DEGRADED",
      "command": "add"
    }
  ],
  "else": [
    {
      "path": "$.systemHealth",
      "value": "HEALTHY",
      "command": "add"
    }
  ]
}
```

### Performance Optimization
```csharp
// Use specific paths to avoid unnecessary counting
// Good: specific counting
.Add(CountBuilders.Count("$.activeUsers[*]"))

// Less efficient: count then filter
.Add(CountBuilders.Count("$.users[?(@.active == true)]"))
```

## Advanced Usage Patterns

### Nested Counting
```json
{
  "path": "$.nestedAnalysis",
  "value": {
    "totalDepartments": "=count($.organization[*])",
    "totalEmployees": "=count($.organization[*].employees[*])",
    "avgEmployeesPerDept": "=count($.organization[*].employees[*]) / count($.organization[*])",
    "activeDepartments": "=count($.organization[?(@.employees && count(@.employees[*]) > 0)])"
  },
  "command": "add"
}
```

### Conditional Counting with Complex Filters
```json
{
  "path": "$.advancedMetrics",
  "value": {
    "highValueCustomers": "=count($.customers[?(@.totalSpent > 10000 && @.status == 'active')])",
    "recentLargeOrders": "=count($.orders[?(@.amount > 1000 && @.date > $.lastMonth)])",
    "criticalLowStock": "=count($.products[?(@.inventory < @.reorderPoint && @.category == 'essential')])"
  },
  "command": "add"
}
```

### Time-Series Analysis
```csharp
var script = new JLioScript()
    .Add(CountBuilders.Count("$.events[?(@.timestamp >= $.startOfDay)]"))
    .OnPath("$.todayCount")
    .Add(CountBuilders.Count("$.events[?(@.timestamp >= $.startOfWeek)]"))
    .OnPath("$.weekCount")
    .Add(CountBuilders.Count("$.events[?(@.timestamp >= $.startOfMonth)]"))
    .OnPath("$.monthCount");
```

## Common Pitfalls

### Empty Collection Handling
```json
// Always check for empty collections
{
  "path": "$.safeCalculation",
  "command": "ifElse", 
  "condition": "=count($.data[*]) > 0",
  "then": [
    {
      "path": "$.result",
      "value": "=avg($.data[*])",
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

### Path Existence Validation
```csharp
// Validate paths exist before counting
var script = new JLioScript()
    .Add(CountBuilders.Count("$.items[*]"))
    .OnPath("$.itemCount")
    .ConditionalOn("$.items != null");
```

### Performance with Large Datasets
```json
{
  "path": "$.efficientCounting",
  "value": {
    // Efficient: direct counting
    "totalUsers": "=count($.users[*])",
    // Less efficient: complex filtering
    "complexFilter": "=count($.users[?(@.active == true && @.premium == true && @.lastLogin > $.threshold)])"
  },
  "command": "add"
}
```