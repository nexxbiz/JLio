# Format Function Documentation

## Overview

The `Format` function applies formatting to values, primarily designed for date/time formatting but can work with other formattable types. It takes a format string and applies it to the target value, converting dates to custom string representations.

## Syntax

### Function Expression Format
```json
// Format current token
"=format('yyyy-MM-dd')"

// Format specific path value
"=format($.dateValue, 'dd/MM/yyyy HH:mm')"

// Format with custom patterns
"=format('dd-MMM-yyyy')"
```

### Programmatic Usage
```csharp
// Format current token
var formatFunction = new Format("yyyy-MM-dd");

// Format specific path with pattern
var formatFunction = new Format("$.createdDate", "dd/MM/yyyy");
```

### Builder Pattern
```csharp
var formatFunction = FormatBuilders.Format("yyyy-MM-dd");
var pathFunction = FormatBuilders.FormatPath("$.dateValue")
                                 .UsingFormat("dd/MM/yyyy HH:mm:ss");
```

## Parameters

- **Format String (Required)**: The format pattern to apply
- **Path (Optional)**: JSONPath expression to value to format
- **Two Argument Form**: `format(path, formatString)`
- **One Argument Form**: `format(formatString)` - applies to current token

## Date Format Patterns

### Standard Date Formats
```json
// Year formats
"yyyy"      → "2024"       // Four-digit year
"yy"        → "24"         // Two-digit year

// Month formats  
"MM"        → "03"         // Two-digit month
"M"         → "3"          // Single-digit month
"MMM"       → "Mar"        // Abbreviated month name
"MMMM"      → "March"      // Full month name

// Day formats
"dd"        → "15"         // Two-digit day
"d"         → "15"         // Single-digit day
"dddd"      → "Friday"     // Full day name
"ddd"       → "Fri"        // Abbreviated day name

// Time formats
"HH"        → "14"         // 24-hour format (two digits)
"H"         → "14"         // 24-hour format (single digit)
"hh"        → "02"         // 12-hour format (two digits)
"h"         → "2"          // 12-hour format (single digit)
"mm"        → "30"         // Minutes (two digits)
"m"         → "30"         // Minutes (single digit)
"ss"        → "45"         // Seconds (two digits)
"s"         → "45"         // Seconds (single digit)
"tt"        → "PM"         // AM/PM designator
```

## Examples

### Basic Date Formatting
```json
{
  "path": "$.formattedDate",
  "value": "=format($.createdAt, 'yyyy-MM-dd')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "createdAt": "2024-03-15T14:30:45Z"
}
```

**Result**:
```json
{
  "createdAt": "2024-03-15T14:30:45Z",
  "formattedDate": "2024-03-15"
}
```

### Time Formatting
```json
{
  "path": "$.timeOnly",
  "value": "=format($.timestamp, 'HH:mm:ss')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "timestamp": "2024-03-15T14:30:45Z"
}
```

**Result**:
```json
{
  "timestamp": "2024-03-15T14:30:45Z",
  "timeOnly": "14:30:45"
}
```

### Custom Date Format
```json
{
  "path": "$.readableDate",
  "value": "=format($.eventDate, 'dddd, MMMM dd, yyyy')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "eventDate": "2024-03-15T14:30:45Z"
}
```

**Result**:
```json
{
  "eventDate": "2024-03-15T14:30:45Z",
  "readableDate": "Friday, March 15, 2024"
}
```

### European Date Format
```json
{
  "path": "$.europeanDate",
  "value": "=format($.date, 'dd/MM/yyyy')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "date": "2024-03-15T14:30:45Z"
}
```

**Result**:
```json
{
  "date": "2024-03-15T14:30:45Z",
  "europeanDate": "15/03/2024"
}
```

### 12-Hour Time Format
```json
{
  "path": "$.displayTime",
  "value": "=format($.timestamp, 'h:mm:ss tt')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "timestamp": "2024-03-15T14:30:45Z"
}
```

**Result**:
```json
{
  "timestamp": "2024-03-15T14:30:45Z",
  "displayTime": "2:30:45 PM"
}
```

### Combined Date and Time
```json
{
  "path": "$.fullDateTime",
  "value": "=format($.createdAt, 'yyyy-MM-dd HH:mm:ss')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "createdAt": "2024-03-15T14:30:45Z"
}
```

**Result**:
```json
{
  "createdAt": "2024-03-15T14:30:45Z",
  "fullDateTime": "2024-03-15 14:30:45"
}
```

### Using Without Path (Current Token)
```json
// Applied to a date token
{
  "path": "@.formatted",
  "value": "=format('MMM dd, yyyy')",
  "command": "add"
}
```

When applied to date token `"2024-03-15T14:30:45Z"`:
```json
{
  "originalDate": "2024-03-15T14:30:45Z",
  "formatted": "Mar 15, 2024"
}
```

## Advanced Usage

### File Naming with Dates
```json
{
  "path": "$.fileName",
  "value": "=concat('report_', format($.generatedAt, 'yyyyMMdd_HHmmss'), '.pdf')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "generatedAt": "2024-03-15T14:30:45Z"
}
```

**Result**:
```json
{
  "generatedAt": "2024-03-15T14:30:45Z",
  "fileName": "report_20240315_143045.pdf"
}
```

### Log Timestamp Formatting
```json
{
  "path": "$.logTimestamp",
  "value": "=concat('[', format($.timestamp, 'yyyy-MM-dd HH:mm:ss.fff'), ']')",
  "command": "add"
}
```

### Multiple Format Outputs
```json
[
  {
    "path": "$.dateShort",
    "value": "=format($.date, 'yyyy-MM-dd')",
    "command": "add"
  },
  {
    "path": "$.dateLong",
    "value": "=format($.date, 'dddd, MMMM dd, yyyy')",
    "command": "add"
  },
  {
    "path": "$.timeOnly",
    "value": "=format($.date, 'HH:mm')",
    "command": "add"
  }
]
```

## Fluent API Usage

### Basic Formatting
```csharp
var script = new JLioScript()
    .Add(FormatBuilders.Format("yyyy-MM-dd"))
    .OnPath("$.formattedDate");
```

### Path-Based Formatting
```csharp
var script = new JLioScript()
    .Add(FormatBuilders.FormatPath("$.createdAt")
                       .UsingFormat("dd/MM/yyyy HH:mm"))
    .OnPath("$.displayDate");
```

### Multiple Formats
```csharp
var script = new JLioScript()
    .Add(FormatBuilders.FormatPath("$.timestamp")
                       .UsingFormat("yyyy-MM-dd"))
    .OnPath("$.dateOnly")
    .Add(FormatBuilders.FormatPath("$.timestamp")
                       .UsingFormat("HH:mm:ss"))
    .OnPath("$.timeOnly");
```

## Integration Examples

### Report Generation
```csharp
var script = new JLioScript()
    .Add(FormatBuilders.FormatPath("$.reportDate")
                       .UsingFormat("MMMM yyyy"))
    .OnPath("$.report.title")
    .Add(FormatBuilders.FormatPath("$.generatedAt")
                       .UsingFormat("yyyy-MM-dd HH:mm:ss"))
    .OnPath("$.report.timestamp")
    .Add(ConcatBuilders.Concat(
        "Generated on ",
        "=format($.generatedAt, 'dddd, MMMM dd, yyyy')"
    ))
    .OnPath("$.report.footer");
```

### Data Export
```csharp
var script = new JLioScript()
    .Add(FormatBuilders.FormatPath("$.user.createdAt")
                       .UsingFormat("yyyy-MM-dd"))
    .OnPath("$.user.joinDate")
    .Add(FormatBuilders.FormatPath("$.user.lastLogin")
                       .UsingFormat("dd/MM/yyyy HH:mm"))
    .OnPath("$.user.lastAccess")
    .Add(ConcatBuilders.Concat(
        "Member since ",
        "=format($.user.createdAt, 'MMMM yyyy')"
    ))
    .OnPath("$.user.membershipInfo");
```

### Audit Logging
```csharp
var script = new JLioScript()
    .Add(FormatBuilders.FormatPath("$.audit.timestamp")
                       .UsingFormat("yyyy-MM-dd HH:mm:ss.fff"))
    .OnPath("$.audit.formattedTime")
    .Add(ConcatBuilders.Concat(
        "[",
        "=format($.audit.timestamp, 'HH:mm:ss')",
        "] ",
        "=fetch($.audit.action)",
        " by ",
        "=fetch($.audit.user)"
    ))
    .OnPath("$.audit.logEntry");
```

## Supported Input Types

### Date Strings
```json
// ISO 8601 formats
"2024-03-15T14:30:45Z"           // UTC with Z
"2024-03-15T14:30:45.123Z"       // With milliseconds
"2024-03-15T14:30:45+00:00"      // With timezone offset

// Other recognizable formats
"2024-03-15 14:30:45"            // Space-separated
"03/15/2024"                     // US format
"15/03/2024"                     // EU format
```

### Date Objects
```json
// JToken Date types are automatically handled
```

### String Parsing
The function attempts to parse string values as dates using `DateTime.TryParse()`.

## Error Handling

### Invalid Date Strings
```json
{
  "path": "$.result",
  "value": "=format($.invalidDate, 'yyyy-MM-dd')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "invalidDate": "not-a-date"
}
```

**Result**: Returns original value unchanged if parsing fails
```json
{
  "invalidDate": "not-a-date",
  "result": "not-a-date"
}
```

### Non-Date Types
```json
{
  "path": "$.result",
  "value": "=format($.numberValue, 'yyyy-MM-dd')",
  "command": "add"
}
```

For non-date types that can't be parsed, the original value is returned.

### Invalid Format Strings
Invalid format patterns may cause exceptions or unexpected output.

## Performance Considerations

- **Date Parsing**: String-to-date conversion adds overhead
- **Format Application**: Custom formatting requires string manipulation
- **Culture Sensitivity**: Date formatting is culture-aware
- **Memory Usage**: Creates new string representations

## Best Practices

1. **Validate Input**: Ensure input values are valid dates or date strings
2. **Test Format Strings**: Verify format patterns produce expected output
3. **Handle Errors**: Plan for invalid date inputs
4. **Consider Culture**: Be aware of culture-specific date formatting
5. **Document Formats**: Clearly specify expected input and output formats
6. **Performance Testing**: Test with large datasets if used frequently
7. **Timezone Awareness**: Consider timezone implications in date formatting

## Common Format Patterns

### File-Friendly Formats
```json
"yyyyMMdd"              → "20240315"
"yyyyMMdd_HHmmss"       → "20240315_143045"
"yyyy-MM-dd_HH-mm-ss"   → "2024-03-15_14-30-45"
```

### Display Formats
```json
"dd/MM/yyyy"            → "15/03/2024"
"MM/dd/yyyy"            → "03/15/2024"
"MMMM dd, yyyy"         → "March 15, 2024"
"dddd, dd MMMM yyyy"    → "Friday, 15 March 2024"
```

### Technical Formats
```json
"yyyy-MM-ddTHH:mm:ss"   → "2024-03-15T14:30:45"
"yyyy-MM-dd HH:mm:ss"   → "2024-03-15 14:30:45"
"yyyyMMddHHmmss"        → "20240315143045"
```

### Time-Only Formats
```json
"HH:mm"                 → "14:30"
"HH:mm:ss"              → "14:30:45"
"h:mm tt"               → "2:30 PM"
"hh:mm:ss tt"           → "02:30:45 PM"
```