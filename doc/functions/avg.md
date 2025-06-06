# Avg Function Documentation

## Overview

The `Avg` function calculates the arithmetic mean (average) of numeric values from multiple sources including literal values, JSONPath expressions, and arrays. It provides the same flexible input handling as the Sum function but returns the average instead of the total.

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
// Average literal values
"=avg(1, 2, 3)"

// Average from JSONPath expressions
"=avg($.score1, $.score2, $.score3)"

// Average array elements
"=avg($.numbers[*])"

// Average nested array elements
"=avg($.students[*].grades[*])"

// Mixed arguments
"=avg(85, $.midterm, $.finals[*])"
```

### Programmatic Usage
```csharp
// With literal arguments
var avgFunction = new Avg("1", "2", "3");

// Empty constructor for dynamic arguments
var avgFunction = new Avg();
```

### Builder Pattern
```csharp
var avgFunction = AvgBuilders.Avg("1", "3");
var pathAvg = AvgBuilders.Avg("$.scores[*]");
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

### Basic Average Calculation
```json
{
  "path": "$.averageScore",
  "value": "=avg($.exam1, $.exam2, $.exam3)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "exam1": 85,
  "exam2": 92,
  "exam3": 78
}
```

**Result**:
```json
{
  "exam1": 85,
  "exam2": 92,
  "exam3": 78,
  "averageScore": 85
}
```

### Array Average
```json
{
  "path": "$.meanScore",
  "value": "=avg($.scores[*])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "scores": [88, 92, 85, 90, 95]
}
```

**Result**:
```json
{
  "scores": [88, 92, 85, 90, 95],
  "meanScore": 90
}
```

### Employee Performance Average
```json
{
  "path": "$.averageRating",
  "value": "=avg($.employees[*].performanceRating)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "employees": [
    {"name": "John", "performanceRating": 4.2},
    {"name": "Jane", "performanceRating": 4.8},
    {"name": "Bob", "performanceRating": 3.9}
  ]
}
```

**Result**:
```json
{
  "employees": [...],
  "averageRating": 4.3
}
```

### Nested Structure Average
```json
{
  "path": "$.averageResponseTime",
  "value": "=avg($.regions[*].servers[*].responseTime)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "regions": [
    {
      "name": "North",
      "servers": [
        {"id": "server1", "responseTime": 120},
        {"id": "server2", "responseTime": 150}
      ]
    },
    {
      "name": "South",
      "servers": [
        {"id": "server3", "responseTime": 110},
        {"id": "server4", "responseTime": 140}
      ]
    }
  ]
}
```

**Result**:
```json
{
  "regions": [...],
  "averageResponseTime": 130
}
```

### Mixed Type Average
```json
{
  "path": "$.averageValue",
  "value": "=avg(100, $.stringNumber, $.arrayNumbers[*])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "stringNumber": "80",
  "arrayNumbers": [90, 110]
}
```

**Result**:
```json
{
  "stringNumber": "80",
  "arrayNumbers": [90, 110],
  "averageValue": 95
}
```

### Student Grade Average
```json
{
  "path": "$.gpa",
  "value": "=avg($.courses[*].grade)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "student": "Alice Johnson",
  "courses": [
    {"name": "Mathematics", "grade": 92},
    {"name": "Science", "grade": 88},
    {"name": "History", "grade": 95},
    {"name": "English", "grade": 90}
  ]
}
```

**Result**:
```json
{
  "student": "Alice Johnson",
  "courses": [...],
  "gpa": 91.25
}
```

### Financial Average Calculations
```json
{
  "path": "$.averageTransactionAmount",
  "value": "=avg($.transactions[*].amount)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "transactions": [
    {"id": 1, "amount": 250.00},
    {"id": 2, "amount": 175.50},
    {"id": 3, "amount": 320.75},
    {"id": 4, "amount": 198.25}
  ]
}
```

**Result**:
```json
{
  "transactions": [...],
  "averageTransactionAmount": 236.125
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
"=avg('10', '20', '30')"  // Result: 20
```

### Arrays
Processes arrays recursively:
```json
// Flat array
"=avg([4, 6, 8])"  // Result: 6

// Nested arrays
"=avg([[2, 4], [6, 8]])"  // Result: 5

// Object arrays
"=avg([{score: 85}, {score: 95}])"  // Averages 'score' properties: 90
```

## Advanced Usage

### Performance Analytics
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.metrics[*].responseTime"))
    .OnPath("$.averageResponseTime")
    .Add(AvgBuilders.Avg("$.metrics[*].throughput"))
    .OnPath("$.averageThroughput")
    .Add(AvgBuilders.Avg("$.metrics[*].errorRate"))
    .OnPath("$.averageErrorRate");
```

### Sales Performance Analysis
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.salespeople[*].monthlySales"))
    .OnPath("$.averageMonthlySales")
    .Add(AvgBuilders.Avg("$.products[*].rating"))
    .OnPath("$.averageProductRating")
    .Add(AvgBuilders.Avg("$.customers[*].satisfactionScore"))
    .OnPath("$.averageCustomerSatisfaction");
```

### Resource Utilization Monitoring
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.servers[*].cpu.utilization"))
    .OnPath("$.averageCpuUtilization")
    .Add(AvgBuilders.Avg("$.servers[*].memory.utilization"))
    .OnPath("$.averageMemoryUtilization")
    .Add(AvgBuilders.Avg("$.servers[*].disk.utilization"))
    .OnPath("$.averageDiskUtilization");
```

## Fluent API Usage

### Basic Average
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("1", "3", "5"))
    .OnPath("$.average");
```

### Path-Based Average
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.test1", "$.test2", "$.test3"))
    .OnPath("$.averageTest");
```

### Array Processing
```csharp
var script = new JLioScript()
    .Add(AvgBuilders.Avg("$.grades[*]"))
    .OnPath("$.gpa")
    .Add(AvgBuilders.Avg("$.attendanceRates[*]"))
    .OnPath("$.averageAttendance");
```

## Calculation Logic

### Average Formula
```
Average = Sum of all values / Count of values
```

### Empty Input Handling
```json
"=avg()"  // Returns: 0 (avoids division by zero)
```

### Null Value Handling
Null values are excluded from both sum and count:
```json
"=avg(10, null, 20, null, 30)"  // Result: 20 (sum=60, count=3)
```

### Single Value
```json
"=avg(42)"  // Returns: 42
```

## Error Handling

### Invalid Data Types
```json
{
  "path": "$.result",
  "value": "=avg($.invalidValue)",
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
Log: "avg can only handle numeric values or arrays. Current type = String"
```

### Mixed Valid and Invalid Types
If any argument contains invalid data, the entire function fails:
```json
"=avg(1, 2, 'invalid')"  // Fails due to invalid string
```

### Division by Zero Prevention
The function handles empty input gracefully:
```json
"=avg([])"  // Returns: 0 (not NaN or error)
```

## Performance Considerations

### Large Datasets
- Processing time scales linearly with data size
- Memory usage for intermediate calculations
- Consider sampling for very large datasets

### Nested Structures
- Deep nesting requires recursive traversal
- JSONPath wildcards scan entire structures
- Use specific paths for better performance

### Precision
- Uses double-precision floating-point arithmetic
- May have rounding errors with very large datasets
- Consider decimal precision requirements

## Best Practices

1. **Data Validation**: Ensure all input values are numeric
2. **Performance Optimization**: Use specific JSONPath expressions
3. **Handle Edge Cases**: Plan for empty datasets and null values
4. **Error Monitoring**: Check execution results and logs
5. **Precision Awareness**: Consider floating-point precision limitations
6. **Testing**: Test with various data distributions and sizes
7. **Documentation**: Document expected data ranges and precision

## Integration Examples

### Student Report Card
```csharp
var script = new JLioScript()
    // Calculate subject averages
    .Add(AvgBuilders.Avg("$.math.tests[*]"))
    .OnPath("$.math.average")
    .Add(AvgBuilders.Avg("$.science.tests[*]"))
    .OnPath("$.science.average")
    .Add(AvgBuilders.Avg("$.english.tests[*]"))
    .OnPath("$.english.average")
    // Calculate overall GPA
    .Add(AvgBuilders.Avg("$.math.average", "$.science.average", "$.english.average"))
    .OnPath("$.overallGPA");
```

### Performance Dashboard
```csharp
var script = new JLioScript()
    // Application metrics
    .Add(AvgBuilders.Avg("$.applications[*].responseTime"))
    .OnPath("$.metrics.averageResponseTime")
    .Add(AvgBuilders.Avg("$.applications[*].throughput"))
    .OnPath("$.metrics.averageThroughput")
    // Server metrics
    .Add(AvgBuilders.Avg("$.servers[*].cpuUsage"))
    .OnPath("$.metrics.averageCpuUsage")
    .Add(AvgBuilders.Avg("$.servers[*].memoryUsage"))
    .OnPath("$.metrics.averageMemoryUsage");
```

### Sales Analytics
```csharp
var script = new JLioScript()
    // Team performance
    .Add(AvgBuilders.Avg("$.salesTeam[*].monthlyRevenue"))
    .OnPath("$.analytics.averageTeamRevenue")
    .Add(AvgBuilders.Avg("$.salesTeam[*].closingRate"))
    .OnPath("$.analytics.averageClosingRate")
    // Product performance
    .Add(AvgBuilders.Avg("$.products[*].customerRating"))
    .OnPath("$.analytics.averageProductRating")
    .Add(AvgBuilders.Avg("$.products[*].profitMargin"))
    .OnPath("$.analytics.averageProfitMargin");
```

## Common Patterns

### Subject Average Pattern
```json
"=avg($.tests[*].score)"
```

### Multi-Source Average Pattern
```json
"=avg($.quiz1, $.quiz2, $.midterm, $.final)"
```

### Nested Performance Pattern
```json
"=avg($.departments[*].employees[*].rating)"
```

### Time-Series Average Pattern
```json
"=avg($.dailyMetrics[*].value)"
```

### Weighted Average Simulation
```json
// Note: This is simple average, not weighted
// For weighted averages, use combination of sum() functions
"=avg($.scores[*])"
```

## Comparison with Other Functions

### Avg vs Sum
- **Avg**: Returns mean value, good for typical/representative values
- **Sum**: Returns total, good for cumulative amounts

### Avg vs Count
- **Avg**: Calculates central tendency of values
- **Count**: Calculates quantity of items

### Use Avg When:
- Analyzing performance metrics
- Calculating grade point averages
- Measuring typical response times
- Determining average transaction amounts
- Benchmarking against standards