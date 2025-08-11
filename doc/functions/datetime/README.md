# DateTime Functions Documentation

This directory contains comprehensive documentation for all JLio date and time functions.

## Available Functions

### Date/Time Generation
- [datetime](datetime.md) - Generate current or formatted date/time values
- [newGuid](newGuid.md) - Generate unique identifiers (GUID/UUID)

### Future Extensions
- [dateAdd](dateAdd.md) - Add time intervals to dates *(planned)*
- [dateDiff](dateDiff.md) - Calculate differences between dates *(planned)*
- [dateFormat](dateFormat.md) - Format dates with custom patterns *(planned)*
- [parseDate](parseDate.md) - Parse date strings into date objects *(planned)*

## Installation

### Extension Pack Registration
```csharp
// DateTime functions are included in the core functions
// No separate registration required for datetime() and newGuid()

// Register all functions including datetime
var parseOptions = ParseOptions.CreateDefault();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Key Features

### Flexible Time Selection
The `datetime` function supports various time options:
- Current local time
- UTC time
- Start of day (local or UTC)
- Custom formatting

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
    .Add(DatetimeBuilders.Datetime("yyyy-MM-dd"))
    .OnPath("$.processDate");
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
- Tracking data lineage
- Batch processing identification
- Audit trail creation

### API Responses
- Response timestamps
- Request correlation IDs
- Cache expiration times
- Rate limiting windows

### Reporting
- Report generation timestamps
- Data snapshot dates
- Scheduled job execution times
- Performance monitoring

### Database Operations
- Primary key generation
- Created/updated timestamps
- Version tracking
- Correlation tracking

## Format Patterns

### Standard Formats
```json
"=datetime()"                           // 2024-03-15T14:30:45.123Z (ISO 8601)
"=datetime('UTC')"                      // 2024-03-15T19:30:45.123Z (UTC ISO 8601)
"=datetime('yyyy-MM-dd')"               // 2024-03-15 (Date only)
"=datetime('HH:mm:ss')"                 // 14:30:45 (Time only)
```

### Custom Formats
```json
"=datetime('MMM dd, yyyy')"             // Mar 15, 2024
"=datetime('dd/MM/yyyy HH:mm')"         // 15/03/2024 14:30
"=datetime('yyyy-MM-ddTHH:mm:ssZ')"     // 2024-03-15T14:30:45Z
"=datetime('yyyyMMdd_HHmmss')"          // 20240315_143045
```

### Localized Formats
```json
"=datetime('dddd, MMMM dd, yyyy')"      // Friday, March 15, 2024
"=datetime('ddd MMM dd HH:mm:ss yyyy')" // Fri Mar 15 14:30:45 2024
```

## Integration Examples

### With String Functions
```json
{
  "path": "$.filename",
  "value": "=concat('report_', datetime('yyyyMMdd'), '.csv')",
  "command": "add"
}
```

### With Conditional Logic
```json
{
  "path": "$.processing",
  "value": {
    "startedAt": "=datetime('UTC')",
    "batchId": "=newGuid()",
    "status": "processing"
  },
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
    .Add("$.currentUser")
    .OnPath("$.audit.modifiedBy");
```

### Event Logging Pattern
```json
[
  {
    "path": "$.events",
    "value": {
      "eventId": "=newGuid()",
      "timestamp": "=datetime('UTC')",
      "type": "user_action",
      "data": "@"
    },
    "command": "add"
  }
]
```

## Best Practices

### Consistency
1. **UTC Usage**: Use UTC for system timestamps to avoid timezone issues
2. **Format Standardization**: Use consistent date formats across your application
3. **ID Generation**: Use GUIDs for distributed system correlation

### Performance
1. **Caching**: Cache datetime values when processing multiple records
2. **Format Efficiency**: Use simple formats when possible
3. **Batch Processing**: Generate timestamps once per batch when appropriate

### Data Quality
1. **Timezone Awareness**: Always specify timezone context
2. **Format Documentation**: Document expected date formats
3. **Validation**: Validate date ranges and formats as needed

## Error Handling

DateTime functions are generally robust but consider:
- Format string validation
- Timezone availability
- System time accuracy
- GUID uniqueness (extremely rare collisions)

## Future Extensions

Planned datetime functions include:
- **dateAdd**: Add days, hours, minutes to dates
- **dateDiff**: Calculate time differences
- **dateFormat**: Advanced formatting options
- **parseDate**: Parse various date string formats
- **dateValidate**: Validate date strings and ranges
- **dateCompare**: Compare dates with operators

These extensions will provide comprehensive date/time manipulation capabilities for complex data processing scenarios.