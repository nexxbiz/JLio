# maxDate Function Documentation

## Overview

The `maxDate` function finds the maximum (latest) date from multiple date values. It's essential for finding the most recent event, determining latest activity timestamps, and analyzing temporal data ranges.

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
"=maxDate('2024-01-01', '2024-12-31', '2024-06-15')"

// Array of dates
"=maxDate($.dates[*])"

// Mixed arguments and arrays  
"=maxDate($.startDate, $.events[*].timestamp)"
```

### Programmatic Usage
```csharp
// Multiple date arguments
var maxDateFunction = new MaxDate("'2024-01-01'", "'2024-12-31'");

// Empty constructor for dynamic arguments
var maxDateFunction = new MaxDate();
```

### Builder Pattern
```csharp
var maxDateFunction = MaxDateBuilders.MaxDate("'2024-01-01'", "'2024-12-31'");
var arrayMaxDate = MaxDateBuilders.MaxDate("$.dates[*]");
```

## Parameters

- **dates** (variable): One or more date values to compare
  - **Type**: String, Date, Array of dates, JSONPath expressions
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps
  - **Required**: At least one valid date

## Return Value

- **Type**: String (ISO 8601 format)
- **Format**: `"yyyy-MM-ddTHH:mm:ss.fffffffZ"` 
- **Example**: `"2024-12-31T00:00:00.0000000Z"`

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
  "path": "$.latestDate",
  "value": "=maxDate('2024-01-01', '2024-12-31', '2024-06-15')",
  "command": "add"
}
```

**Result**:
```json
{
  "latestDate": "2024-12-31T00:00:00.0000000Z"
}
```

### Find Latest Event
```json
{
  "path": "$.analytics.lastActivity",
  "value": "=maxDate($.events[*].timestamp)",
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
    "lastActivity": "2024-01-20T14:45:00.0000000Z"
  }
}
```

### Data Range Analysis
```json
{
  "path": "$.dateRange.end",
  "value": "=maxDate($.records[*].createdAt)",
  "command": "add"
}
```

### Mixed Date Sources
```json
{
  "path": "$.latestUpdate",
  "value": "=maxDate($.user.lastLogin, $.profile.updatedAt, $.settings.modifiedAt)",
  "command": "add"
}
```

### Log Analysis
```json
{
  "path": "$.logs.latestError",
  "value": "=maxDate($.logs[?(@.level == 'ERROR')].timestamp)",
  "command": "add"
}
```

## Advanced Usage

### Fluent API Processing
```csharp
var script = new JLioScript()
    .Add(MaxDateBuilders.MaxDate("$.orders[*].orderDate"))
    .OnPath("$.analytics.lastOrderDate")
    .Add(MaxDateBuilders.MaxDate("$.users[*].lastActiveDate"))  
    .OnPath("$.analytics.lastUserActivity");
```

### Combined with Other Functions
```json
{
  "path": "$.summary.timeSpan",
  "value": "=concat('Latest: ', maxDate($.events[*].date))",
  "command": "add"
}
```

### Conditional Processing
```json
{
  "path": "$.isRecent",
  "value": "=dateCompare(maxDate($.activities[*].timestamp), datetime()) >= -7",
  "command": "add"
}
```

## Data Type Handling

### Date Objects
```json
"=maxDate($.dateField1, $.dateField2)"  // Direct date comparison
```

### String Dates
```json
"=maxDate('2024-01-15', '2024-02-20')"  // String parsing
```

### Arrays
```json
"=maxDate($.timestamps[*])"  // Array processing
```

### Unix Timestamps
```json
"=maxDate(1705334400, 1708012800)"  // Integer timestamps
```

### Mixed Types
```json
"=maxDate('2024-01-15', 1705334400, $.dateField)"  // Automatic conversion
```

## Error Handling

### No Arguments
```json
"=maxDate()"  // Error: Requires at least one argument
```

### Invalid Dates
```json
"=maxDate('invalid-date', '2024-01-01')"  // Error: Cannot parse date
```

### Empty Arrays
```json
"=maxDate($.emptyArray[*])"  // Error: No valid dates found
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

### Latest Activity Pattern
```json
"=maxDate($.user.lastLogin, $.user.lastPurchase, $.user.lastView)"
```

### Event Timeline Pattern
```json
"=maxDate($.timeline[*].eventDate)"
```

### Data Freshness Pattern  
```json
"=maxDate($.cache[*].lastUpdated)"
```

### Audit Trail Pattern
```json
"=maxDate($.audit[*].modifiedAt)"
```

## Integration Examples

### With Date Comparison
```json
{
  "path": "$.isLatest",
  "value": "=dateCompare(maxDate($.versions[*].releaseDate), $.currentVersion.date) == 0",
  "command": "add"
}
```

### With Date Range Validation
```json
{
  "path": "$.maxInRange", 
  "value": "=maxDate($.events[?isDateBetween(@.date, '2024-01-01', '2024-12-31')].date)",
  "command": "add"
}
```

### With String Functions
```json
{
  "path": "$.summary",
  "value": "=concat('Data through: ', maxDate($.records[*].date))",
  "command": "add"
}
```

## Use Case Examples

### E-commerce Analytics
```csharp
var script = new JLioScript()
    .Add(MaxDateBuilders.MaxDate("$.orders[*].orderDate"))
    .OnPath("$.analytics.lastOrderDate")
    .Add(MaxDateBuilders.MaxDate("$.products[*].lastSold"))
    .OnPath("$.analytics.lastSaleDate");
```

### User Activity Tracking
```json
{
  "path": "$.user.lastActivity",
  "value": "=maxDate($.user.sessions[*].endTime)",
  "command": "add"
}
```

### System Monitoring
```json
[
  {
    "path": "$.monitoring.lastHealthCheck",
    "value": "=maxDate($.services[*].lastPing)",
    "command": "add"
  },
  {
    "path": "$.monitoring.lastAlert",
    "value": "=maxDate($.alerts[*].timestamp)",
    "command": "add"
  }
]
```

### Content Management
```json
{
  "path": "$.content.lastModified",
  "value": "=maxDate($.pages[*].updatedAt, $.media[*].uploadedAt)",
  "command": "add"
}
```

## Cross-Platform Behavior

The `maxDate` function uses culture-independent date parsing to ensure consistent behavior across:
- Windows, Linux, macOS
- Different system locales
- Docker containers
- Cloud environments

All date parsing uses `CultureInfo.InvariantCulture` for reliable cross-platform operation.

## See Also

- [minDate](minDate.md) - Find the earliest date
- [avgDate](avgDate.md) - Calculate average date  
- [dateCompare](dateCompare.md) - Compare two dates
- [isDateBetween](isDateBetween.md) - Check date ranges
- [datetime](../datetime.md) - Generate current timestamps