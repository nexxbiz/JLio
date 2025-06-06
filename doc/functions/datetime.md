# Datetime Function Documentation

## Overview

The `Datetime` function generates date and time values with various configuration options for time zones, date selections, and custom formatting. It's essential for timestamping data, creating time-based identifiers, and date calculations.

## Syntax

### Function Expression Formats
```json
// Basic - current local time
"=datetime()"

// UTC time
"=datetime('UTC')"

// Custom format
"=datetime('dd-MM-yyyy HH:mm:ss')"

// UTC with custom format
"=datetime('UTC', 'dd-MM-yyyy HH:mm:ss')"

// Start of day variations
"=datetime('startOfDay')"
"=datetime('startOfDayUTC')"
"=datetime('startOfDay', 'yyyy-MM-dd')"
```

### Programmatic Usage
```csharp
// No arguments - current local time
var datetimeFunction = new Datetime();

// With arguments
var datetimeFunction = new Datetime("UTC", "yyyy-MM-dd");
```

### Builder Pattern
```csharp
var datetimeFunction = DatetimeBuilders.Datetime();
var utcFunction = DatetimeBuilders.Datetime("UTC");
var formattedFunction = DatetimeBuilders.Datetime("UTC", "dd-MM-yyyy");
```

## Parameters

### Time Selection (First Parameter)
- **`now`** (default): Current local time
- **`UTC`**: Current UTC time
- **`startOfDay`**: Beginning of current day (00:00:00) in local time
- **`startOfDayUTC`**: Beginning of current day (00:00:00) in UTC

### Format String (Second Parameter)
- **Default**: `"yyyy-MM-ddTHH:mm:ss.fffZ"` (ISO 8601 format)
- **Custom**: Any valid .NET DateTime format string

## Examples

### Basic Date/Time Generation
```json
{
  "path": "$.timestamp",
  "value": "=datetime()",
  "command": "add"
}
```

**Result**: 
```json
{
  "timestamp": "2024-03-15T14:30:45.123Z"
}
```

### UTC Time
```json
{
  "path": "$.utcTimestamp",
  "value": "=datetime('UTC')",
  "command": "add"
}
```

**Result**:
```json
{
  "utcTimestamp": "2024-03-15T19:30:45.123Z"
}
```

### Custom Formatting
```json
{
  "path": "$.formattedDate",
  "value": "=datetime('dd-MM-yyyy HH:mm:ss')",
  "command": "add"
}
```

**Result**:
```json
{
  "formattedDate": "15-03-2024 14:30:45"
}
```

### Start of Day
```json
{
  "path": "$.dayStart",
  "value": "=datetime('startOfDay', 'yyyy-MM-dd')",
  "command": "add"
}
```

**Result**:
```json
{
  "dayStart": "2024-03-15"
}
```

### UTC Start of Day
```json
{
  "path": "$.utcDayStart", 
  "value": "=datetime('startOfDayUTC', 'yyyy-MM-dd HH:mm:ss')",
  "command": "add"
}
```

**Result**:
```json
{
  "utcDayStart": "2024-03-15 00:00:00"
}
```

## Format String Examples

### Common Date Formats
```json
// ISO 8601 (default)
"=datetime('UTC')"  // "2024-03-15T19:30:45.123Z"

// Date only
"=datetime('yyyy-MM-dd')"  // "2024-03-15"

// European format
"=datetime('dd/MM/yyyy')"  // "15/03/2024"

// US format  
"=datetime('MM/dd/yyyy')"  // "03/15/2024"

// Custom readable format
"=datetime('dddd, MMMM dd, yyyy')"  // "Friday, March 15, 2024"

// Time only
"=datetime('HH:mm:ss')"  // "14:30:45"

// 12-hour format
"=datetime('h:mm:ss tt')"  // "2:30:45 PM"
```

### Technical Formats
```json
// Unix timestamp style
"=datetime('yyyy-MM-dd HH:mm:ss')"  // "2024-03-15 14:30:45"

// Log format
"=datetime('[yyyy-MM-dd HH:mm:ss.fff]')"  // "[2024-03-15 14:30:45.123]"

// File naming
"=datetime('yyyyMMdd_HHmmss')"  // "20240315_143045"
```

## Use Cases

### Audit Timestamps
```json
[
  {
    "path": "$.audit.createdAt",
    "value": "=datetime('UTC')",
    "command": "add"
  },
  {
    "path": "$.audit.createdBy",
    "value": "system",
    "command": "add"
  }
]
```

### File Processing
```json
{
  "path": "$.file.processedAt",
  "value": "=datetime('UTC', 'yyyy-MM-ddTHH:mm:ss.fffZ')",
  "command": "add"
}
```

### Daily Reports
```json
{
  "path": "$.report.reportDate",
  "value": "=datetime('startOfDay', 'yyyy-MM-dd')",
  "command": "add"
}
```

### Log Entries
```json
{
  "path": "$.log.timestamp",
  "value": "=datetime('[yyyy-MM-dd HH:mm:ss.fff]')",
  "command": "add"
}
```

## Fluent API Usage

### Basic Usage
```csharp
var script = new JLioScript()
    .Add(DatetimeBuilders.Datetime())
    .OnPath("$.timestamp");
```

### With Parameters
```csharp
var script = new JLioScript()
    .Add(DatetimeBuilders.Datetime("UTC", "yyyy-MM-dd"))
    .OnPath("$.dateOnly");
```

### Complex Combinations
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat("BATCH_", "=datetime('yyyyMMdd_HHmmss')"))
    .OnPath("$.batchId")
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.createdAt");
```

## Integration Examples

### Data Processing Pipeline
```csharp
var script = new JLioScript()
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.processing.startedAt")
    .Add(new JValue("processing"))
    .OnPath("$.status")
    .Merge("$.incomingData")
    .With("$.currentData")
    .UsingDefaultSettings()
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.processing.completedAt")
    .Set(new JValue("completed"))
    .OnPath("$.status");
```

### Entity Lifecycle Tracking
```csharp
var script = new JLioScript()
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.entity.id")
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.entity.createdAt")
    .Copy("$.entity.createdAt")
    .To("$.entity.modifiedAt")
    .Add(new JValue(1))
    .OnPath("$.entity.version");
```

### Report Generation
```csharp
var script = new JLioScript()
    .Add(DatetimeBuilders.Datetime("startOfDay", "yyyy-MM-dd"))
    .OnPath("$.report.reportDate")
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.report.generatedAt")
    .Add(new JValue("daily-summary"))
    .OnPath("$.report.type");
```

## Error Handling

### Invalid Time Selection
```csharp
// Unknown time selection falls back to format interpretation
"=datetime('invalid')"  // Treats 'invalid' as format string, uses current time
```

### Invalid Format String
```csharp
// Invalid format strings may cause exceptions
// Function logs warnings and falls back to default format
```

### Argument Count
```csharp
// Function accepts 0, 1, or 2 arguments
// Additional arguments are ignored
```

## Time Zone Considerations

- **Local Time**: Uses server/system local time zone
- **UTC**: Coordinated Universal Time, independent of server location
- **Start of Day**: Calculated based on selected time zone (local or UTC)
- **Format Output**: Time zone information included based on format string

## Performance Considerations

- **Generation Speed**: Very fast, minimal overhead
- **Format Parsing**: Complex format strings have slight overhead
- **Memory Usage**: Minimal string allocation
- **Thread Safety**: Safe for concurrent use

## Best Practices

1. **Use UTC for Storage**: Store timestamps in UTC to avoid time zone issues
2. **Consistent Formatting**: Use consistent format strings across your application
3. **Document Format Choices**: Clearly document expected date/time formats
4. **Consider Start of Day**: Use startOfDay for date-only comparisons
5. **Validate Formats**: Test custom format strings thoroughly
6. **Audit Trails**: Always include timestamps in audit and logging data
7. **Time Zone Awareness**: Be explicit about time zone handling in documentation

## Common Patterns

### Entity Creation Pattern
```json
{
  "path": "$.entity.createdAt",
  "value": "=datetime('UTC')",
  "command": "add"
}
```

### Daily Processing Pattern
```json
{
  "path": "$.processing.date",
  "value": "=datetime('startOfDay', 'yyyy-MM-dd')",
  "command": "add"
}
```

### File Naming Pattern
```json
{
  "path": "$.file.name",
  "value": "=concat('export_', datetime('yyyyMMdd_HHmmss'), '.json')",
  "command": "add"
}
```

### Version Timestamping Pattern
```json
{
  "path": "$.version.timestamp",
  "value": "=datetime('UTC', 'yyyy-MM-ddTHH:mm:ss.fffZ')",
  "command": "add"
}
```