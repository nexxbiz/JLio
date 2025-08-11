# isDateBetween Function Documentation

## Overview

The `isDateBetween` function checks if a date falls within a specified range (inclusive). It's essential for date range validation, filtering records by date ranges, and conditional date processing workflows.

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
// Check if date is between two other dates
"=isDateBetween('2024-06-15', '2024-01-01', '2024-12-31')"

// Using JSONPath expressions
"=isDateBetween($.checkDate, $.startDate, $.endDate)"

// Mixed literal and JSONPath
"=isDateBetween($.eventDate, '2024-01-01', '2024-12-31')"
```

### Programmatic Usage
```csharp
// With three date arguments
var isDateBetweenFunction = new IsDateBetween("'2024-06-15'", "'2024-01-01'", "'2024-12-31'");

// Empty constructor for dynamic arguments
var isDateBetweenFunction = new IsDateBetween();
```

### Builder Pattern
```csharp
var isDateBetweenFunction = IsDateBetweenBuilders.IsDateBetween("'2024-06-15'", "'2024-01-01'", "'2024-12-31'");
var dynamicCheck = IsDateBetweenBuilders.IsDateBetween("$.checkDate", "$.startDate", "$.endDate");
```

## Parameters

- **checkDate** (required): The date to check
  - **Type**: String, Date, JSONPath expression
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps
- **startDate** (required): The start of the date range (inclusive)
  - **Type**: String, Date, JSONPath expression
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps  
- **endDate** (required): The end of the date range (inclusive)
  - **Type**: String, Date, JSONPath expression
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps

## Return Value

- **Type**: Boolean (JValue)
- **Value**: `true` if checkDate is between startDate and endDate (inclusive), `false` otherwise

## Inclusive Range Behavior

The function includes both boundary dates:
- `isDateBetween('2024-01-01', '2024-01-01', '2024-12-31')` ? `true`
- `isDateBetween('2024-12-31', '2024-01-01', '2024-12-31')` ? `true`
- `isDateBetween('2023-12-31', '2024-01-01', '2024-12-31')` ? `false`
- `isDateBetween('2025-01-01', '2024-01-01', '2024-12-31')` ? `false`

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

### Basic Range Validation
```json
{
  "path": "$.isInValidPeriod",
  "value": "=isDateBetween('2024-06-15', '2024-01-01', '2024-12-31')",
  "command": "add"
}
```

**Result**:
```json
{
  "isInValidPeriod": true
}
```

### Event Date Validation
```json
{
  "path": "$.events[*].isInCurrentYear",
  "value": "=isDateBetween(@.eventDate, '2024-01-01', '2024-12-31')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "events": [
    {"eventDate": "2024-03-15", "name": "Conference"},
    {"eventDate": "2023-11-20", "name": "Workshop"}, 
    {"eventDate": "2024-08-10", "name": "Seminar"}
  ]
}
```

**Result**:
```json
{
  "events": [
    {"eventDate": "2024-03-15", "name": "Conference", "isInCurrentYear": true},
    {"eventDate": "2023-11-20", "name": "Workshop", "isInCurrentYear": false}, 
    {"eventDate": "2024-08-10", "name": "Seminar", "isInCurrentYear": true}
  ]
}
```

### Dynamic Range Validation
```json
{
  "path": "$.order.isInValidDeliveryWindow",
  "value": "=isDateBetween($.order.requestedDelivery, $.order.earliestDelivery, $.order.latestDelivery)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "order": {
    "requestedDelivery": "2024-03-20",
    "earliestDelivery": "2024-03-15",
    "latestDelivery": "2024-03-25"
  }
}
```

**Result**:
```json
{
  "order": {
    "requestedDelivery": "2024-03-20",
    "earliestDelivery": "2024-03-15", 
    "latestDelivery": "2024-03-25",
    "isInValidDeliveryWindow": true
  }
}
```

### Filtering by Date Range
```json
{
  "path": "$.validRecords",
  "value": "$.records[?isDateBetween(@.date, '2024-01-01', '2024-12-31')]",
  "command": "add"
}
```

### Business Hours Validation
```json
{
  "path": "$.appointments[*].isDuringBusinessHours",
  "value": "=isDateBetween(@.appointmentTime, '2024-01-01T09:00:00', '2024-12-31T17:00:00')",
  "command": "add"
}
```

## Advanced Usage

### Fluent API Processing
```csharp
var script = new JLioScript()
    .Add(IsDateBetweenBuilders.IsDateBetween("@.startDate", "$.project.earliestStart", "$.project.latestStart"))
    .OnPath("$.tasks[*].isValidStartDate")
    .Add(IsDateBetweenBuilders.IsDateBetween("@.dueDate", "$.project.minDeadline", "$.project.maxDeadline"))
    .OnPath("$.tasks[*].isValidDueDate");
```

### Multi-Range Validation
```json
[
  {
    "path": "$.validation.isInQ1",
    "value": "=isDateBetween($.date, '2024-01-01', '2024-03-31')",
    "command": "add"
  },
  {
    "path": "$.validation.isInQ2", 
    "value": "=isDateBetween($.date, '2024-04-01', '2024-06-30')",
    "command": "add"
  },
  {
    "path": "$.validation.isInCurrentHalf",
    "value": "=isDateBetween($.date, '2024-01-01', '2024-06-30')",
    "command": "add"
  }
]
```

### Conditional Processing Based on Date Range
```json
{
  "path": "$.status",
  "value": "=if(isDateBetween($.dueDate, datetime(), '2024-12-31'), 'ACTIVE', 'EXPIRED')",
  "command": "add"
}
```

## Data Type Handling

### Date Objects
```json
"=isDateBetween($.dateField, $.startField, $.endField)"  // Direct date comparison
```

### String Dates
```json
"=isDateBetween('2024-06-15', '2024-01-01', '2024-12-31')"  // String parsing
```

### Unix Timestamps
```json
"=isDateBetween(1718409600, 1704067200, 1735689600)"  // Integer timestamps
```

### Mixed Types
```json
"=isDateBetween('2024-06-15', 1704067200, $.endDate)"  // Automatic conversion
```

## Error Handling

### Wrong Argument Count
```json
"=isDateBetween('2024-01-01', '2024-12-31')"  // Error: Requires exactly 3 arguments
```

### Invalid Dates
```json
"=isDateBetween('invalid-date', '2024-01-01', '2024-12-31')"  // Error: Cannot parse date
```

### Null/Missing Dates
```json
"=isDateBetween($.missingDate, '2024-01-01', '2024-12-31')"  // Error: Invalid date value
```

## Performance Considerations

- **Date Parsing**: ISO 8601 formats parse fastest
- **Comparison Logic**: Simple date comparison operations
- **Memory Usage**: Minimal memory overhead for validation
- **Culture Independence**: Consistent performance across locales

## Best Practices

1. **Date Formats**: Use ISO 8601 format for optimal performance and reliability
2. **Range Logic**: Ensure start date is before or equal to end date
3. **Validation**: Validate that all three dates are valid before comparison
4. **Error Handling**: Handle cases where date parsing fails
5. **Timezone**: Be consistent with timezone usage across all dates

## Common Patterns

### Date Range Filter Pattern
```json
"=isDateBetween(@.date, $.filter.startDate, $.filter.endDate)"
```

### Business Period Validation Pattern
```json
"=isDateBetween($.transactionDate, $.fiscalYear.start, $.fiscalYear.end)"
```

### Event Scheduling Pattern  
```json
"=isDateBetween($.event.date, $.venue.availableFrom, $.venue.availableTo)"
```

### Data Retention Pattern
```json
"=isDateBetween($.record.createdAt, $.policy.retentionStart, datetime())"
```

## Integration Examples

### With Other Date Functions
```json
{
  "path": "$.analysis.hasRecentActivity",
  "value": "=isDateBetween(maxDate($.activities[*].timestamp), '2024-01-01', datetime())",
  "command": "add"
}
```

### With String Functions
```json
{
  "path": "$.status",
  "value": "=if(isDateBetween($.date, '2024-01-01', '2024-12-31'), 'CURRENT', 'ARCHIVED')",
  "command": "add"
}
```

### With Math Functions
```json
{
  "path": "$.metrics.activeCount",
  "value": "=count($.items[?isDateBetween(@.lastActive, '2024-01-01', datetime())])",
  "command": "add"
}
```

## Use Case Examples

### E-commerce Order Processing
```csharp
var script = new JLioScript()
    .Add(IsDateBetweenBuilders.IsDateBetween("@.orderDate", "$.campaign.startDate", "$.campaign.endDate"))
    .OnPath("$.orders[*].isInCampaignPeriod")
    .Add(IsDateBetweenBuilders.IsDateBetween("@.deliveryDate", "$.shippingWindow.start", "$.shippingWindow.end"))
    .OnPath("$.orders[*].isValidDelivery");
```

### Employee Attendance Validation
```json
{
  "path": "$.attendance[*].isValidWorkday",
  "value": "=isDateBetween(@.clockIn, $.workSchedule.start, $.workSchedule.end)",
  "command": "add"
}
```

### Financial Reporting Period
```json
[
  {
    "path": "$.transactions[*].isInReportingPeriod",
    "value": "=isDateBetween(@.transactionDate, $.report.periodStart, $.report.periodEnd)",
    "command": "add"
  },
  {
    "path": "$.summary.transactionsInPeriod",
    "value": "=count($.transactions[?(@.isInReportingPeriod == true)])",
    "command": "add"
  }
]
```

### Event Management System
```json
{
  "path": "$.events[*].registration",
  "value": {
    "isRegistrationOpen": "=isDateBetween(datetime(), @.registrationStart, @.registrationEnd)",
    "isEventActive": "=isDateBetween(datetime(), @.eventStart, @.eventEnd)"
  },
  "command": "add"
}
```

### Data Quality Monitoring
```json
{
  "path": "$.dataQuality.recordsFreshness",
  "value": {
    "freshRecords": "=count($.records[?isDateBetween(@.lastUpdated, '2024-01-01', datetime())])",
    "staleRecords": "=count($.records[?!isDateBetween(@.lastUpdated, '2024-01-01', datetime())])"
  },
  "command": "add"
}
```

## Cross-Platform Behavior

The `isDateBetween` function uses culture-independent date parsing to ensure consistent behavior across:
- Windows, Linux, macOS
- Different system locales
- Docker containers
- Cloud environments

All date parsing uses `CultureInfo.InvariantCulture` for reliable cross-platform operation.

## See Also

- [maxDate](maxDate.md) - Find the latest date
- [minDate](minDate.md) - Find the earliest date  
- [avgDate](avgDate.md) - Calculate average date
- [dateCompare](dateCompare.md) - Compare two dates
- [datetime](../datetime.md) - Generate current timestamps