# DateTime Functions Documentation

This directory contains comprehensive documentation for all JLio date and time functions.

## Available Functions

### Date/Time Generation
- [datetime](datetime.md) - Generate current or formatted date/time values
- [newGuid](newGuid.md) - Generate unique identifiers (GUID/UUID)

### Date Analysis & Comparison
- [maxDate](maxDate.md) - Find the maximum (latest) date from multiple values
- [minDate](minDate.md) - Find the minimum (earliest) date from multiple values  
- [avgDate](avgDate.md) - Calculate average date from multiple values
- [isDateBetween](isDateBetween.md) - Check if date falls within a range (inclusive)
- [dateCompare](dateCompare.md) - Compare two dates (-1, 0, 1)

### Future Extensions
- [dateAdd](dateAdd.md) - Add time intervals to dates *(planned)*
- [dateDiff](dateDiff.md) - Calculate differences between dates *(planned)*
- [dateFormat](dateFormat.md) - Format dates with custom patterns *(planned)*
- [parseDate](parseDate.md) - Parse date strings into date objects *(planned)*

## Installation

### Extension Pack Registration
```csharp
// Register TimeDate functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterTimeDate();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);

// Note: datetime() and newGuid() are part of core functions
// The new date analysis functions require RegisterTimeDate()
```

## Key Features

### Flexible Time Selection
The `datetime` function supports various time options:
- Current local time
- UTC time
- Start of day (local or UTC)
- Custom formatting

### Date Analysis
New date analysis functions provide powerful data processing capabilities:
- Find earliest/latest dates in datasets
- Calculate temporal center points
- Validate date ranges
- Compare dates programmatically

### Cross-Platform Reliability
All date functions use culture-independent parsing for consistent behavior across:
- Windows, Linux, macOS
- Different system locales
- Docker containers
- Cloud environments

### Timezone Support
Built-in support for different timezone contexts:
```json
"=datetime()"                    // Local time
"=datetime('UTC')"               // UTC time
"=datetime('startOfDay')"        // Local start of day
"=datetime('startOfDayUTC')"     // UTC start of day
```

### Custom Formatting
Extensive formatting options using .NET DateTime format strings:
```json
"=datetime('yyyy-MM-dd')"              // 2024-03-15
"=datetime('UTC', 'dd-MM-yyyy HH:mm')" // 15-03-2024 14:30
"=datetime('MMM dd, yyyy')"            // Mar 15, 2024
```

### Unique Identifiers
Generate globally unique identifiers for data processing:
```json
"=newGuid()"  // "4d2c4ec7-30ca-4eea-aeb3-2154fb02eb1d"
```

## Usage Patterns

### Timestamping Data
```json
{
  "path": "$.timestamp",
  "value": "=datetime()",
  "command": "add"
}
```

### Data Range Analysis
```json
[
  {
    "path": "$.analytics.earliestEvent",
    "value": "=minDate($.events[*].timestamp)",
    "command": "add"
  },
  {
    "path": "$.analytics.latestEvent", 
    "value": "=maxDate($.events[*].timestamp)",
    "command": "add"
  },
  {
    "path": "$.analytics.typicalEventTime",
    "value": "=avgDate($.events[*].timestamp)",
    "command": "add"
  }
]
```

### Date Range Validation
```json
{
  "path": "$.validation.isValidPeriod",
  "value": "=isDateBetween($.checkDate, $.period.start, $.period.end)",
  "command": "add"
}
```

### Record Creation
```json
{
  "path": "$.record",
  "value": {
    "id": "=newGuid()",
    "createdAt": "=datetime('UTC')",
    "createdDate": "=datetime('yyyy-MM-dd')"
  },
  "command": "add"
}
```

### Batch Processing
```csharp
var script = new JLioScript()
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.processedAt")
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.batchId")
    .Add(MaxDateBuilders.MaxDate("$.records[*].lastModified"))
    .OnPath("$.batchLatestRecord");
```

### Log Entry Generation
```json
[
  {
    "path": "$.logs[*].timestamp",
    "value": "=datetime('UTC', 'yyyy-MM-ddTHH:mm:ss.fffZ')",
    "command": "add"
  },
  {
    "path": "$.logs[*].id",
    "value": "=newGuid()",
    "command": "add"
  }
]
```

## Common Use Cases

### Data Processing
- Timestamping processed records
- Finding date ranges in datasets
- Calculating temporal statistics
- Tracking data lineage
- Batch processing identification
- Audit trail creation

### Analytics & Reporting
- Event timeline analysis
- User activity patterns
- Performance trend analysis
- Data freshness monitoring
- Historical data analysis

### API Responses
- Response timestamps
- Request correlation IDs
- Cache expiration times
- Rate limiting windows

### Database Operations
- Primary key generation
- Created/updated timestamps
- Version tracking
- Correlation tracking

## Date Format Support

### Supported Input Formats (Culture-Independent)
```json
// ISO 8601 (recommended)
"2024-01-15T10:30:00Z"
"2024-01-15T10:30:00.123Z"
"2024-01-15"

// Unambiguous formats
"15-Jan-2024"
"Jan 15, 2024" 
"2024/01/15"

// Unix timestamps
1705334400
```

### Standard Output Formats
```json
"=datetime()"                           // 2024-03-15T14:30:45.123Z (ISO 8601)
"=maxDate($.dates[*])"                  // 2024-12-31T00:00:00.0000000Z
"=datetime('UTC')"                      // 2024-03-15T19:30:45.123Z (UTC ISO 8601)
"=datetime('yyyy-MM-dd')"               // 2024-03-15 (Date only)
```

### Custom Formats
```json
"=datetime('MMM dd, yyyy')"             // Mar 15, 2024
"=datetime('dd/MM/yyyy HH:mm')"         // 15/03/2024 14:30
"=datetime('yyyy-MM-ddTHH:mm:ssZ')"     // 2024-03-15T14:30:45Z
"=datetime('yyyyMMdd_HHmmss')"          // 20240315_143045
```

## Integration Examples

### With String Functions
```json
{
  "path": "$.summary",
  "value": "=concat('Data range: ', minDate($.records[*].date), ' to ', maxDate($.records[*].date))",
  "command": "add"
}
```

### With Math Functions
```json
{
  "path": "$.analysis",
  "value": {
    "totalRecords": "=count($.data[*])",
    "dateRange": "=concat(minDate($.data[*].timestamp), ' - ', maxDate($.data[*].timestamp))",
    "averageDate": "=avgDate($.data[*].timestamp)"
  },
  "command": "add"
}
```

### With Conditional Logic
```json
{
  "path": "$.status",
  "value": "=if(dateCompare($.dueDate, datetime()) < 0, 'OVERDUE', 'ACTIVE')",
  "command": "add"
}
```

### Audit Trail Pattern
```csharp
var script = new JLioScript()
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.audit.modifiedAt")
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.audit.changeId")
    .Add(MaxDateBuilders.MaxDate("$.history[*].timestamp"))
    .OnPath("$.audit.lastHistoryEntry");
```

### Event Logging Pattern
```json
[
  {
    "path": "$.events",
    "value": {
      "eventId": "=newGuid()",
      "timestamp": "=datetime('UTC')",
      "type": "data_analysis",
      "dateRange": {
        "start": "=minDate($.processedData[*].date)",
        "end": "=maxDate($.processedData[*].date)"
      }
    },
    "command": "add"
  }
]
```

## Best Practices

### Consistency
1. **UTC Usage**: Use UTC for system timestamps to avoid timezone issues
2. **Format Standardization**: Use ISO 8601 formats for data interchange
3. **ID Generation**: Use GUIDs for distributed system correlation
4. **Date Analysis**: Use culture-independent parsing for reliable results

### Performance
1. **Function Selection**: Choose the right function for your use case
2. **Format Efficiency**: Use simple formats when possible
3. **Array Processing**: Leverage array syntax for efficient date processing
4. **Error Handling**: Validate date inputs and handle parsing errors

### Data Quality
1. **Timezone Awareness**: Always specify timezone context
2. **Format Documentation**: Document expected date formats
3. **Validation**: Use isDateBetween for range validation
4. **Comparison**: Use dateCompare for reliable date ordering

## Error Handling

DateTime functions include comprehensive error handling:
- **Argument Validation**: Required argument count checking
- **Date Parsing**: Culture-independent parsing with detailed error messages
- **Type Conversion**: Automatic handling of various date formats
- **Range Checking**: Validation of date values and ranges
- **Detailed Logging**: Error messages for debugging

Common error scenarios:
- Invalid date formats
- Missing required arguments
- Empty date arrays
- Null/undefined date values

## Migration from Core DateTime

If you were using the core `datetime` function and want to add date analysis capabilities:

```csharp
// Before - only basic datetime
var parseOptions = ParseOptions.CreateDefault();

// After - with date analysis functions
var parseOptions = ParseOptions.CreateDefault().RegisterTimeDate();
```

The core `datetime` and `newGuid` functions remain available and unchanged.

## Future Extensions

Planned datetime functions include:
- **dateAdd**: Add days, hours, minutes to dates
- **dateDiff**: Calculate time differences between dates
- **dateFormat**: Advanced formatting with culture support
- **parseDate**: Parse various date string formats with validation
- **dateValidate**: Validate date strings and ranges
- **dateArithmetic**: Complex date calculations

These extensions will provide comprehensive date/time manipulation capabilities for advanced data processing scenarios.

## See Also

- [Functions Overview](../README.md) - All available function packages
- [Math Functions](../math/) - Numeric processing functions  
- [Text Functions](../text/) - String manipulation functions
- [Core Functions](../) - Essential data functions