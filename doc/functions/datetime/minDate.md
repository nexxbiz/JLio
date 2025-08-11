# minDate Function Documentation

## Overview

The `minDate` function finds the minimum (earliest) date from multiple date values. It's essential for finding the oldest event, determining earliest activity timestamps, and analyzing temporal data ranges.

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
"=minDate('2024-01-01', '2024-12-31', '2024-06-15')"

// Array of dates
"=minDate($.dates[*])"

// Mixed arguments and arrays  
"=minDate($.startDate, $.events[*].timestamp)"
```

### Programmatic Usage
```csharp
// Multiple date arguments
var minDateFunction = new MinDate("'2024-01-01'", "'2024-12-31'");

// Empty constructor for dynamic arguments
var minDateFunction = new MinDate();
```

### Builder Pattern
```csharp
var minDateFunction = MinDateBuilders.MinDate("'2024-01-01'", "'2024-12-31'");
var arrayMinDate = MinDateBuilders.MinDate("$.dates[*]");
```

## Parameters

- **dates** (variable): One or more date values to compare
  - **Type**: String, Date, Array of dates, JSONPath expressions
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps
  - **Required**: At least one valid date

## Return Value

- **Type**: String (ISO 8601 format)
- **Format**: `"yyyy-MM-ddTHH:mm:ss.fffffffZ"` 
- **Example**: `"2024-01-01T00:00:00.0000000Z"`

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
  "path": "$.earliestDate",
  "value": "=minDate('2024-01-01', '2024-12-31', '2024-06-15')",
  "command": "add"
}
```

**Result**:
```json
{
  "earliestDate": "2024-01-01T00:00:00.0000000Z"
}
```

### Find First Event
```json
{
  "path": "$.analytics.firstActivity",
  "value": "=minDate($.events[*].timestamp)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "events": [
    {"timestamp": "2024-01-15T10:30:00Z", "type": "login"},
    {"timestamp": "2024-01-20T14:45:00Z", "type": "purchase"}, 
    {"timestamp": "2024-01-18T09:15:00Z", "type": "view"}
  ]
}
```

**Result**:
```json
{
  "events": [...],
  "analytics": {
    "firstActivity": "2024-01-15T10:30:00.0000000Z"
  }
}
```

### Data Range Analysis
```json
{
  "path": "$.dateRange.start",
  "value": "=minDate($.records[*].createdAt)",
  "command": "add"
}
```

### Account Creation Tracking
```json
{
  "path": "$.firstRegistration",
  "value": "=minDate($.users[*].registeredAt)",
  "command": "add"
}
```

### Historical Data Analysis
```json
{
  "path": "$.logs.earliestEntry",
  "value": "=minDate($.logs[*].timestamp)",
  "command": "add"
}
```

## Advanced Usage

### Fluent API Processing
```csharp
var script = new JLioScript()
    .Add(MinDateBuilders.MinDate("$.orders[*].orderDate"))
    .OnPath("$.analytics.firstOrderDate")
    .Add(MinDateBuilders.MinDate("$.users[*].joinDate"))  
    .OnPath("$.analytics.firstUserJoined");
```

### Combined with Other Functions
```json
{
  "path": "$.summary.startDate",
  "value": "=concat('Started: ', minDate($.projects[*].startDate))",
  "command": "add"
}
```

### Time Range Calculation
```json
[
  {
    "path": "$.period.start",
    "value": "=minDate($.activities[*].timestamp)",
    "command": "add"
  },
  {
    "path": "$.period.end", 
    "value": "=maxDate($.activities[*].timestamp)",
    "command": "add"
  }
]
```

## Data Type Handling

### Date Objects
```json
"=minDate($.dateField1, $.dateField2)"  // Direct date comparison
```

### String Dates
```json
"=minDate('2024-01-15', '2024-02-20')"  // String parsing
```

### Arrays
```json
"=minDate($.timestamps[*])"  // Array processing
```

### Unix Timestamps
```json
"=minDate(1705334400, 1708012800)"  // Integer timestamps
```

### Mixed Types
```json
"=minDate('2024-01-15', 1705334400, $.dateField)"  // Automatic conversion
```

## Error Handling

### No Arguments
```json
"=minDate()"  // Error: Requires at least one argument
```

### Invalid Dates
```json
"=minDate('invalid-date', '2024-01-01')"  // Error: Cannot parse date
```

### Empty Arrays
```json
"=minDate($.emptyArray[*])"  // Error: No valid dates found
```

## Performance Considerations

- **Array Size**: Performance scales linearly with number of dates
- **Date Parsing**: ISO 8601 formats parse fastest
- **Memory Usage**: Minimal memory overhead for date comparison
- **Culture Independence**: Consistent performance across locales

## Best Practices

1. **Date Formats**: Use ISO 8601 format for optimal performance and reliability
2. **Validation**: Ensure date sources contain valid date data
3. **Arrays**: Leverage array syntax for processing multiple dates efficiently
4. **Error Handling**: Check for null/empty date collections
5. **Timezone**: Be consistent with timezone usage across date sources

## Common Patterns

### First Activity Pattern
```json
"=minDate($.user.firstLogin, $.user.registrationDate)"
```

### Project Timeline Pattern
```json
"=minDate($.phases[*].startDate)"
```

### Data Baseline Pattern  
```json
"=minDate($.measurements[*].recordedAt)"
```

### System Launch Pattern
```json
"=minDate($.services[*].deployedAt)"
```

## Integration Examples

### With Date Comparison
```json
{
  "path": "$.isFirstVersion",
  "value": "=dateCompare(minDate($.versions[*].releaseDate), $.currentVersion.date) == 0",
  "command": "add"
}
```

### With Date Range Validation
```json
{
  "path": "$.minInRange", 
  "value": "=minDate($.events[?isDateBetween(@.date, '2024-01-01', '2024-12-31')].date)",
  "command": "add"
}
```

### With String Functions
```json
{
  "path": "$.summary",
  "value": "=concat('Data from: ', minDate($.records[*].date))",
  "command": "add"
}
```

## Use Case Examples

### E-commerce Analytics
```csharp
var script = new JLioScript()
    .Add(MinDateBuilders.MinDate("$.orders[*].orderDate"))
    .OnPath("$.analytics.firstOrderDate")
    .Add(MinDateBuilders.MinDate("$.products[*].createdAt"))
    .OnPath("$.analytics.firstProductDate");
```

### User Onboarding Analysis
```json
{
  "path": "$.cohort.earliestUser",
  "value": "=minDate($.users[*].signupDate)",
  "command": "add"
}
```

### System Monitoring
```json
[
  {
    "path": "$.monitoring.deploymentStart",
    "value": "=minDate($.services[*].startTime)",
    "command": "add"
  },
  {
    "path": "$.monitoring.firstAlert",
    "value": "=minDate($.alerts[*].timestamp)",
    "command": "add"
  }
]
```

### Data Migration Tracking
```json
{
  "path": "$.migration.startDate",
  "value": "=minDate($.batches[*].processedAt)",
  "command": "add"
}
```

### Content Timeline
```json
{
  "path": "$.content.establishedDate",
  "value": "=minDate($.articles[*].publishedAt, $.pages[*].createdAt)",
  "command": "add"
}
```

## Cross-Platform Behavior

The `minDate` function uses culture-independent date parsing to ensure consistent behavior across:
- Windows, Linux, macOS
- Different system locales
- Docker containers
- Cloud environments

All date parsing uses `CultureInfo.InvariantCulture` for reliable cross-platform operation.

## See Also

- [maxDate](maxDate.md) - Find the latest date
- [avgDate](avgDate.md) - Calculate average date  
- [dateCompare](dateCompare.md) - Compare two dates
- [isDateBetween](isDateBetween.md) - Check date ranges
- [datetime](../datetime.md) - Generate current timestamps