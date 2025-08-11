# avgDate Function Documentation

## Overview

The `avgDate` function calculates the average (mean) date from multiple date values. It's useful for finding the temporal center point of events, determining typical timing patterns, and statistical analysis of date ranges.

## Installation

### Extension Pack Registration
```csharp
// Register TimeDate functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterTimeDate();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Syntax

### Function Expression Formats
```json
// Multiple date arguments
"=avgDate('2024-01-01', '2024-12-31')"

// Array of dates
"=avgDate($.dates[*])"

// Mixed arguments and arrays  
"=avgDate($.startDate, $.events[*].timestamp)"
```

### Programmatic Usage
```csharp
// Multiple date arguments
var avgDateFunction = new AvgDate("'2024-01-01'", "'2024-12-31'");

// Empty constructor for dynamic arguments
var avgDateFunction = new AvgDate();
```

### Builder Pattern
```csharp
var avgDateFunction = AvgDateBuilders.AvgDate("'2024-01-01'", "'2024-12-31'");
var arrayAvgDate = AvgDateBuilders.AvgDate("$.dates[*]");
```

## Parameters

- **dates** (variable): One or more date values to average
  - **Type**: String, Date, Array of dates, JSONPath expressions
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps
  - **Required**: At least one valid date

## Return Value

- **Type**: String (ISO 8601 format)
- **Format**: `"yyyy-MM-ddTHH:mm:ss.fffffffZ"` 
- **Example**: `"2024-07-01T12:00:00.0000000Z"`

## How It Works

The function calculates the average by:
1. Converting all dates to DateTime ticks (100-nanosecond intervals since Jan 1, 0001)
2. Computing the arithmetic mean of the tick values
3. Converting the average back to a DateTime
4. Formatting as ISO 8601 string

## Supported Date Formats

### Input Formats (Culture-Independent)
```json
// ISO 8601 (recommended)
"2024-01-15T10:30:00Z"
"2024-01-15T10:30:00.123Z"
"2024-01-15"

// Unambiguous formats
"15-Jan-2024"
"Jan 15, 2024" 
"2024/01/15"
```

## Examples

### Basic Usage
```json
{
  "path": "$.averageDate",
  "value": "=avgDate('2024-01-01', '2024-12-31')",
  "command": "add"
}
```

**Result** (approximately mid-year):
```json
{
  "averageDate": "2024-07-01T12:00:00.0000000Z"
}
```

### Event Timeline Analysis
```json
{
  "path": "$.analytics.typicalEventTime",
  "value": "=avgDate($.events[*].timestamp)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "events": [
    {"timestamp": "2024-01-15T10:30:00Z", "type": "login"},
    {"timestamp": "2024-01-20T14:45:00Z", "type": "purchase"}, 
    {"timestamp": "2024-01-25T09:15:00Z", "type": "view"}
  ]
}
```

**Result** (approximate center of the event timeframe):
```json
{
  "events": [...],
  "analytics": {
    "typicalEventTime": "2024-01-20T11:30:00.0000000Z"
  }
}
```

### Project Timeline Center Point
```json
{
  "path": "$.project.midpoint",
  "value": "=avgDate($.project.startDate, $.project.endDate)",
  "command": "add"
}
```

### User Activity Patterns
```json
{
  "path": "$.userStats.averageLoginTime",
  "value": "=avgDate($.user.sessions[*].loginTime)",
  "command": "add"
}
```

### Historical Data Center
```json
{
  "path": "$.dataset.temporalCenter",
  "value": "=avgDate($.measurements[*].recordedAt)",
  "command": "add"
}
```

## Advanced Usage

### Fluent API Processing
```csharp
var script = new JLioScript()
    .Add(AvgDateBuilders.AvgDate("$.orders[*].orderDate"))
    .OnPath("$.analytics.averageOrderDate")
    .Add(AvgDateBuilders.AvgDate("$.deliveries[*].deliveryDate"))  
    .OnPath("$.analytics.averageDeliveryDate");
```

### Statistical Analysis
```json
[
  {
    "path": "$.stats.earliest",
    "value": "=minDate($.events[*].timestamp)",
    "command": "add"
  },
  {
    "path": "$.stats.latest", 
    "value": "=maxDate($.events[*].timestamp)",
    "command": "add"
  },
  {
    "path": "$.stats.average",
    "value": "=avgDate($.events[*].timestamp)",
    "command": "add"
  }
]
```

### Seasonal Analysis
```json
{
  "path": "$.seasonal.typicalPeakDate",
  "value": "=avgDate($.sales[?(@.amount > 1000)].date)",
  "command": "add"
}
```

## Data Type Handling

### Date Objects
```json
"=avgDate($.dateField1, $.dateField2)"  // Direct date averaging
```

### String Dates
```json
"=avgDate('2024-01-15', '2024-02-20')"  // String parsing and averaging
```

### Arrays
```json
"=avgDate($.timestamps[*])"  // Array processing
```

### Unix Timestamps
```json
"=avgDate(1705334400, 1708012800)"  // Integer timestamps
```

### Mixed Types
```json
"=avgDate('2024-01-15', 1705334400, $.dateField)"  // Automatic conversion
```

## Error Handling

### No Arguments
```json
"=avgDate()"  // Error: Requires at least one argument
```

### Invalid Dates
```json
"=avgDate('invalid-date', '2024-01-01')"  // Error: Cannot parse date
```

### Empty Arrays
```json
"=avgDate($.emptyArray[*])"  // Error: No valid dates found
```

## Performance Considerations

- **Array Size**: Performance scales linearly with number of dates
- **Precision**: Uses DateTime ticks for high precision averaging
- **Memory Usage**: Minimal memory overhead for date calculations
- **Culture Independence**: Consistent performance across locales

## Best Practices

1. **Date Formats**: Use ISO 8601 format for optimal performance and reliability
2. **Meaningful Averages**: Ensure the average makes sense for your use case
3. **Large Datasets**: Consider sampling for very large date arrays
4. **Time Zones**: Be consistent with timezone usage across date sources
5. **Validation**: Verify that input dates represent comparable events

## Common Patterns

### Project Midpoint Pattern
```json
"=avgDate($.project.startDate, $.project.endDate)"
```

### Activity Center Pattern
```json
"=avgDate($.activities[*].timestamp)"
```

### Seasonal Analysis Pattern  
```json
"=avgDate($.events[?(@.season == 'summer')].date)"
```

### Performance Baseline Pattern
```json
"=avgDate($.metrics[*].measuredAt)"
```

## Integration Examples

### With Date Comparison
```json
{
  "path": "$.isAboveAverage",
  "value": "=dateCompare($.currentDate, avgDate($.historicalDates[*])) > 0",
  "command": "add"
}
```

### With Date Range Creation
```json
{
  "path": "$.estimatedDate", 
  "value": "=avgDate($.estimates[*].projectedDate)",
  "command": "add"
}
```

### With String Functions
```json
{
  "path": "$.report",
  "value": "=concat('Typical date: ', avgDate($.samples[*].date))",
  "command": "add"
}
```

## Use Case Examples

### Project Management
```csharp
var script = new JLioScript()
    .Add(AvgDateBuilders.AvgDate("$.milestones[*].targetDate"))
    .OnPath("$.project.estimatedMidpoint")
    .Add(AvgDateBuilders.AvgDate("$.tasks[*].completedAt"))
    .OnPath("$.project.averageCompletionDate");
```

### Sales Analytics
```json
{
  "path": "$.sales.typicalOrderDate",
  "value": "=avgDate($.orders[*].orderDate)",
  "command": "add"
}
```

### User Behavior Analysis
```json
[
  {
    "path": "$.cohort.averageSignupDate",
    "value": "=avgDate($.users[*].signupDate)",
    "command": "add"
  },
  {
    "path": "$.cohort.averageFirstPurchase",
    "value": "=avgDate($.users[*].firstPurchaseDate)",
    "command": "add"
  }
]
```

### Quality Metrics
```json
{
  "path": "$.quality.averageTestDate",
  "value": "=avgDate($.testRuns[?(@.result == 'passed')].executedAt)",
  "command": "add"
}
```

### Content Publishing Patterns
```json
{
  "path": "$.content.typicalPublishDate",
  "value": "=avgDate($.articles[*].publishedAt)",
  "command": "add"
}
```

## Mathematical Properties

### Central Tendency
The average date represents the temporal center of your data, similar to arithmetic mean for numbers.

### Outlier Sensitivity  
Like numeric averages, date averages can be affected by extreme outlier dates.

### Precision
The function maintains high precision using DateTime ticks (100-nanosecond resolution).

## Cross-Platform Behavior

The `avgDate` function uses culture-independent date parsing to ensure consistent behavior across:
- Windows, Linux, macOS
- Different system locales
- Docker containers
- Cloud environments

All date parsing uses `CultureInfo.InvariantCulture` for reliable cross-platform operation.

## See Also

- [maxDate](maxDate.md) - Find the latest date
- [minDate](minDate.md) - Find the earliest date  
- [dateCompare](dateCompare.md) - Compare two dates
- [isDateBetween](isDateBetween.md) - Check date ranges
- [datetime](../datetime.md) - Generate current timestamps