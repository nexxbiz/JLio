# dateCompare Function Documentation

## Overview

The `dateCompare` function compares two dates and returns an integer indicating their relationship: -1 (first is earlier), 0 (same), or 1 (first is later). It's essential for date sorting, conditional logic based on date comparisons, and temporal data analysis.

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
// Compare two date literals
"=dateCompare('2024-06-15', '2024-01-01')"

// Using JSONPath expressions
"=dateCompare($.date1, $.date2)"

// Mixed literal and JSONPath
"=dateCompare($.eventDate, '2024-01-01')"
```

### Programmatic Usage
```csharp
// With two date arguments
var dateCompareFunction = new DateCompare("'2024-06-15'", "'2024-01-01'");

// Empty constructor for dynamic arguments
var dateCompareFunction = new DateCompare();
```

### Builder Pattern
```csharp
var dateCompareFunction = DateCompareBuilders.DateCompare("'2024-06-15'", "'2024-01-01'");
var dynamicCompare = DateCompareBuilders.DateCompare("$.date1", "$.date2");
```

## Parameters

- **date1** (required): The first date to compare
  - **Type**: String, Date, JSONPath expression
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps
- **date2** (required): The second date to compare
  - **Type**: String, Date, JSONPath expression  
  - **Formats**: ISO 8601, culture-independent formats, Unix timestamps

## Return Value

- **Type**: Integer (JValue)
- **Values**:
  - **-1**: date1 is earlier than date2
  - **0**: date1 is the same as date2
  - **1**: date1 is later than date2

## Comparison Logic

```json
// Examples of return values
"=dateCompare('2024-01-01', '2024-06-15')"  // Returns: -1 (earlier)
"=dateCompare('2024-06-15', '2024-01-01')"  // Returns:  1 (later)
"=dateCompare('2024-06-15', '2024-06-15')"  // Returns:  0 (same)
```

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

### Basic Date Comparison
```json
{
  "path": "$.comparison",
  "value": "=dateCompare('2024-06-15', '2024-01-01')",
  "command": "add"
}
```

**Result**:
```json
{
  "comparison": 1
}
```

### Conditional Logic Based on Date Comparison
```json
{
  "path": "$.status",
  "value": "=if(dateCompare($.dueDate, datetime()) < 0, 'OVERDUE', 'CURRENT')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "dueDate": "2023-12-31"
}
```

**Result**:
```json
{
  "dueDate": "2023-12-31",
  "status": "OVERDUE"
}
```

### Sorting Logic Implementation
```json
{
  "path": "$.events[*].priority",
  "value": "=dateCompare(@.eventDate, datetime())",
  "command": "add"
}
```

**Input Data**:
```json
{
  "events": [
    {"eventDate": "2024-12-25", "name": "Christmas"},
    {"eventDate": "2023-07-04", "name": "Past Event"}, 
    {"eventDate": "2024-06-15", "name": "Summer Conference"}
  ]
}
```

**Result** (assuming current date is 2024-03-15):
```json
{
  "events": [
    {"eventDate": "2024-12-25", "name": "Christmas", "priority": 1},
    {"eventDate": "2023-07-04", "name": "Past Event", "priority": -1}, 
    {"eventDate": "2024-06-15", "name": "Summer Conference", "priority": 1}
  ]
}
```

### Order Processing
```json
{
  "path": "$.order.deliveryStatus",
  "value": "=if(dateCompare($.order.deliveredDate, $.order.expectedDate) <= 0, 'ON_TIME', 'DELAYED')",
  "command": "add"
}
```

### Version Comparison
```json
{
  "path": "$.software.isLatestVersion",
  "value": "=dateCompare($.software.installedVersion.releaseDate, $.software.availableVersion.releaseDate) == 0",
  "command": "add"
}
```

## Advanced Usage

### Fluent API Processing
```csharp
var script = new JLioScript()
    .Add(DateCompareBuilders.DateCompare("@.startDate", "@.plannedStart"))
    .OnPath("$.projects[*].startComparison")
    .Add(DateCompareBuilders.DateCompare("@.endDate", "@.plannedEnd"))
    .OnPath("$.projects[*].endComparison");
```

### Multi-Level Date Comparison
```json
[
  {
    "path": "$.analysis.vsStartDate",
    "value": "=dateCompare($.currentDate, $.project.startDate)",
    "command": "add"
  },
  {
    "path": "$.analysis.vsEndDate", 
    "value": "=dateCompare($.currentDate, $.project.endDate)",
    "command": "add"
  },
  {
    "path": "$.analysis.projectStatus",
    "value": "=if(dateCompare($.currentDate, $.project.startDate) < 0, 'NOT_STARTED', if(dateCompare($.currentDate, $.project.endDate) > 0, 'COMPLETED', 'IN_PROGRESS'))",
    "command": "add"
  }
]
```

### Data Validation
```json
{
  "path": "$.validation.dateRangeValid",
  "value": "=dateCompare($.startDate, $.endDate) <= 0",
  "command": "add"
}
```

### Performance Tracking
```json
{
  "path": "$.metrics.deliveryPerformance",
  "value": "=if(dateCompare($.actualDelivery, $.promisedDelivery) <= 0, 'EARLY_OR_ONTIME', 'LATE')",
  "command": "add"
}
```

## Data Type Handling

### Date Objects
```json
"=dateCompare($.dateField1, $.dateField2)"  // Direct date comparison
```

### String Dates
```json
"=dateCompare('2024-06-15', '2024-01-01')"  // String parsing and comparison
```

### Unix Timestamps
```json
"=dateCompare(1718409600, 1704067200)"  // Integer timestamps
```

### Mixed Types
```json
"=dateCompare('2024-06-15', 1704067200)"  // Automatic conversion
```

## Error Handling

### Wrong Argument Count
```json
"=dateCompare('2024-01-01')"  // Error: Requires exactly 2 arguments
```

### Invalid Dates
```json
"=dateCompare('invalid-date', '2024-01-01')"  // Error: Cannot parse date
```

### Null/Missing Dates
```json
"=dateCompare($.missingDate, '2024-01-01')"  // Error: Invalid date value
```

## Performance Considerations

- **Date Parsing**: ISO 8601 formats parse fastest
- **Comparison Logic**: Simple date comparison operations using DateTime.Compare
- **Memory Usage**: Minimal memory overhead for comparison
- **Culture Independence**: Consistent performance across locales

## Best Practices

1. **Date Formats**: Use ISO 8601 format for optimal performance and reliability
2. **Null Handling**: Validate that both dates exist before comparison
3. **Timezone**: Be consistent with timezone usage across compared dates
4. **Error Handling**: Handle cases where date parsing fails
5. **Logic Flow**: Use comparison result in conditional statements effectively

## Common Patterns

### Date Range Validation Pattern
```json
"=dateCompare($.startDate, $.endDate) <= 0"
```

### Overdue Check Pattern
```json
"=dateCompare($.dueDate, datetime()) < 0"
```

### Version Freshness Pattern  
```json
"=dateCompare($.lastUpdate, '2024-01-01') >= 0"
```

### Event Status Pattern
```json
"=if(dateCompare($.eventDate, datetime()) > 0, 'FUTURE', 'PAST')"
```

## Integration Examples

### With Other Date Functions
```json
{
  "path": "$.analysis.isLatestEvent",
  "value": "=dateCompare($.event.date, maxDate($.allEvents[*].date)) == 0",
  "command": "add"
}
```

### With String Functions
```json
{
  "path": "$.timelineStatus",
  "value": "=concat('Event is ', if(dateCompare($.eventDate, datetime()) > 0, 'upcoming', 'past'))",
  "command": "add"
}
```

### With Math Functions
```json
{
  "path": "$.metrics.futureEventsCount",
  "value": "=count($.events[?dateCompare(@.date, datetime()) > 0])",
  "command": "add"
}
```

## Use Case Examples

### Project Management Timeline
```csharp
var script = new JLioScript()
    .Add(DateCompareBuilders.DateCompare("@.actualStart", "@.plannedStart"))
    .OnPath("$.tasks[*].startVariance")
    .Add(DateCompareBuilders.DateCompare("@.actualEnd", "@.plannedEnd"))
    .OnPath("$.tasks[*].endVariance");
```

### Order Processing System
```json
{
  "path": "$.orders[*].fulfillment",
  "value": {
    "deliveryStatus": "=if(dateCompare(@.deliveredAt, @.promisedAt) <= 0, 'ON_TIME', 'DELAYED')",
    "processingSpeed": "=if(dateCompare(@.processedAt, @.orderAt) == 0, 'SAME_DAY', 'MULTI_DAY')"
  },
  "command": "add"
}
```

### Content Management
```json
[
  {
    "path": "$.articles[*].publishStatus",
    "value": "=if(dateCompare(@.publishDate, datetime()) <= 0, 'PUBLISHED', 'SCHEDULED')",
    "command": "add"
  },
  {
    "path": "$.articles[*].contentAge",
    "value": "=if(dateCompare(@.lastModified, '2024-01-01') >= 0, 'RECENT', 'OLD')",
    "command": "add"
  }
]
```

### Financial Transaction Analysis
```json
{
  "path": "$.transactions[*].timing",
  "value": {
    "isRecentTransaction": "=dateCompare(@.transactionDate, '2024-01-01') >= 0",
    "comparedToLastMonth": "=dateCompare(@.transactionDate, '2024-02-01')",
    "statusVsDueDate": "=if(dateCompare(@.transactionDate, @.dueDate) <= 0, 'TIMELY', 'OVERDUE')"
  },
  "command": "add"
}
```

### Event Scheduling System
```json
{
  "path": "$.events[*].scheduling",
  "value": {
    "registrationStatus": "=if(dateCompare(datetime(), @.registrationDeadline) < 0, 'OPEN', 'CLOSED')",
    "eventStatus": "=if(dateCompare(datetime(), @.eventDate) < 0, 'UPCOMING', if(dateCompare(datetime(), @.eventEndDate) > 0, 'COMPLETED', 'ONGOING'))",
    "isPostponed": "=dateCompare(@.eventDate, @.originalDate) != 0"
  },
  "command": "add"
}
```

## Sorting and Ordering

### Custom Sort Logic
```json
// Create sort keys for custom ordering
{
  "path": "$.items[*].sortKey",
  "value": "=dateCompare(@.priority_date, datetime())",
  "command": "add"
}
```

### Priority Assignment
```json
{
  "path": "$.tasks[*].urgency",
  "value": "=if(dateCompare(@.dueDate, datetime()) < 0, 3, if(dateCompare(@.dueDate, '2024-06-01') <= 0, 2, 1))",
  "command": "add"
}
```

## Cross-Platform Behavior

The `dateCompare` function uses culture-independent date parsing to ensure consistent behavior across:
- Windows, Linux, macOS
- Different system locales
- Docker containers
- Cloud environments

All date parsing uses `CultureInfo.InvariantCulture` for reliable cross-platform operation.

## See Also

- [maxDate](maxDate.md) - Find the latest date
- [minDate](minDate.md) - Find the earliest date  
- [avgDate](avgDate.md) - Calculate average date
- [isDateBetween](isDateBetween.md) - Check date ranges
- [datetime](../datetime.md) - Generate current timestamps