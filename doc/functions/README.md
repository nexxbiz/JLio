# JLio Functions Documentation

This directory contains comprehensive documentation for all JLio functions, organized by functional packages.

## Function Packages

JLio functions are organized into logical packages based on their functionality:

### ?? [Text Functions](text/)
Advanced string manipulation and processing functions.

**Basic Operations**
- [length](text/length.md) - Get string length
- [substring](text/substring.md) - Extract substring with negative index support
- [indexOf](text/indexOf.md) - Find substring position
- [replace](text/replace.md) - Replace text with case sensitivity options

**String Formatting**
- [trim](text/trim.md), [trimStart](text/trimStart.md), [trimEnd](text/trimEnd.md) - Remove whitespace/characters
- [toUpper](text/toUpper.md), [toLower](text/toLower.md) - Case conversion with culture support
- [padLeft](text/padLeft.md), [padRight](text/padRight.md) - Pad strings with characters

**String Testing**
- [contains](text/contains.md), [startsWith](text/startsWith.md), [endsWith](text/endsWith.md) - Text matching
- [isEmpty](text/isEmpty.md) - Check for empty values

**Array/String Conversion**  
- [split](text/split.md) - Split strings into arrays
- [join](text/join.md) - Join arrays into strings

**Legacy Functions**
- [concat](concat.md) - Concatenate strings
- [format](format.md) - String formatting
- [parse](parse.md) - Parse JSON strings  
- [toString](toString.md) - Convert values to strings

### ?? [Math Functions](math/)
Mathematical operations, statistics, and numeric processing.

**Arithmetic**
- [sum](math/sum.md) - Sum numeric values with string number support
- [avg](math/avg.md) - Calculate arithmetic mean
- [subtract](math/subtract.md) - Subtract values

**Statistics**
- [min](math/min.md), [max](math/max.md) - Find minimum/maximum values
- [median](math/median.md) - Calculate median value
- [count](math/count.md) - Count elements

**Mathematical Operations**
- [abs](math/abs.md) - Absolute value
- [pow](math/pow.md) - Power calculations
- [sqrt](math/sqrt.md) - Square root
- [round](math/round.md), [floor](math/floor.md), [ceiling](math/ceiling.md) - Rounding functions

**Advanced**
- [calculate](math/calculate.md) - Complex mathematical expressions

### ?? [DateTime Functions](datetime/)
Date, time, and unique identifier generation.

**Time Generation**
- [datetime](datetime.md) - Generate formatted date/time values
- [newGuid](newGuid.md) - Generate unique identifiers (GUID/UUID)

**Date Analysis & Comparison**
- [maxDate](datetime/maxDate.md) - Find the latest date from multiple values
- [minDate](datetime/minDate.md) - Find the earliest date from multiple values  
- [avgDate](datetime/avgDate.md) - Calculate average date from multiple values
- [isDateBetween](datetime/isDateBetween.md) - Check if date falls within a range
- [dateCompare](datetime/dateCompare.md) - Compare two dates (-1, 0, 1)

**Future Extensions**
- dateAdd, dateDiff, parseDate, dateFormat *(coming soon)*

### ?? Core Functions
Essential data manipulation functions built into the core.

**Data Extraction**
- [fetch](fetch.md) - Extract values from JSON paths
- [partial](partial.md) - Create subsets of data
- [promote](promote.md) - Move nested data up levels

**Data Validation**
- [filterBySchema](filterBySchema.md) - JSON Schema validation and filtering

## Installation by Package

### All Functions
```csharp
var parseOptions = ParseOptions.CreateDefault()
    .RegisterMath()        // Math functions
    .RegisterText()        // Text functions  
    .RegisterTimeDate();   // DateTime functions
    
// Core functions are always available
```

### Individual Packages
```csharp
// Math functions only
var parseOptions = ParseOptions.CreateDefault().RegisterMath();

// Text functions only  
var parseOptions = ParseOptions.CreateDefault().RegisterText();

// DateTime functions only
var parseOptions = ParseOptions.CreateDefault().RegisterTimeDate();

// Core functions only (default)
var parseOptions = ParseOptions.CreateDefault();
```

## Function Categories by Use Case

### ?? Data Processing
- **Aggregation**: sum, avg, min, max, count, median
- **Transformation**: calculate, round, abs, pow, sqrt
- **Extraction**: fetch, partial, promote
- **Validation**: filterBySchema, isEmpty

### ?? Text Processing
- **Cleaning**: trim, trimStart, trimEnd, replace
- **Formatting**: toUpper, toLower, padLeft, padRight
- **Parsing**: split, substring, indexOf
- **Generation**: join, concat, format
- **Validation**: contains, startsWith, endsWith, length

### ?? Temporal Operations
- **Timestamping**: datetime with various formats
- **Identification**: newGuid for unique IDs
- **Date Analysis**: maxDate, minDate, avgDate for data analysis
- **Date Validation**: isDateBetween, dateCompare for range checking

### ?? Data Conversion
- **String Conversion**: toString, format, join
- **Parsing**: parse, split
- **Type Conversion**: Automatic in math functions

## Advanced Usage Patterns

### Multi-Package Integration
```csharp
var script = new JLioScript()
    // Text processing
    .Add(TrimBuilders.Trim("$.userInput"))
    .OnPath("$.cleaned")
    // Math calculation  
    .Add(SumBuilders.Sum("$.amounts[*]"))
    .OnPath("$.total")
    // DateTime analysis
    .Add(MaxDateBuilders.MaxDate("$.events[*].timestamp"))
    .OnPath("$.latestEvent")
    // DateTime stamping
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.processedAt");
```

### Complex Data Pipeline
```json
[
  {
    "path": "$.processing.id",
    "value": "=newGuid()",
    "command": "add"
  },
  {
    "path": "$.processing.startTime", 
    "value": "=datetime('UTC')",
    "command": "add"
  },
  {
    "path": "$.dateRange.earliest",
    "value": "=minDate($.events[*].timestamp)",
    "command": "add"
  },
  {
    "path": "$.dateRange.latest", 
    "value": "=maxDate($.events[*].timestamp)",
    "command": "add"
  },
  {
    "path": "$.cleanedData",
    "value": "=trim(replace($.rawData, '\n', ' '))",
    "command": "add"
  },
  {
    "path": "$.metrics.total",
    "value": "=sum($.values[*])",
    "command": "add" 
  },
  {
    "path": "$.metrics.average",
    "value": "=round(avg($.values[*]), 2)",
    "command": "add"
  }
]
```

### Date Range Validation
```json
{
  "path": "$.validation.isInRange",
  "value": "=isDateBetween($.eventDate, $.period.start, $.period.end)",
  "command": "add"
}
```

### Conditional Function Usage
```json
{
  "path": "$.result",
  "value": "=if(dateCompare($.startDate, $.endDate) <= 0, 'Valid', 'Invalid Date Range')",
  "command": "add"
}
```

## Performance Considerations

### Function Selection
- **Text Functions**: Optimized for string operations, culture-aware
- **Math Functions**: Support string-to-number conversion, array processing
- **DateTime Functions**: Culture-independent parsing, timezone-aware
- **Core Functions**: Highly optimized for data extraction and validation

### Best Practices
1. **Package Selection**: Only register needed function packages
2. **Chaining**: Chain related functions for efficient processing
3. **Data Types**: Leverage automatic type conversion where appropriate
4. **Memory Usage**: Consider memory impact with large datasets
5. **Error Handling**: Monitor execution logs for function errors

## Error Handling

All function packages include comprehensive error handling:
- **Argument Validation**: Count and type checking
- **Type Conversion**: Graceful handling of incompatible types
- **Bounds Checking**: Array and string index validation
- **Culture Support**: Consistent behavior across locales
- **Detailed Logging**: Error messages for debugging

## Migration Guide

### From Legacy Functions
Many functions have been enhanced or moved between packages:

```csharp
// Old approach
"=concat($.a, ' ', $.b)"

// Enhanced with new text functions
"=join(' ', $.a, $.b)"        // More flexible
"=trim(concat($.a, $.b))"     // Combined operations

// New date analysis capabilities
"=maxDate($.events[*].timestamp)"    // Find latest event
"=isDateBetween($.date, $.start, $.end)"  // Validate ranges
```

### Package Evolution
- **Text Package**: Expanded with advanced string operations
- **Math Package**: Enhanced with string number support
- **DateTime Package**: New date analysis and comparison functions
- **Core Package**: Stable foundation functions

## Documentation Standards

Each function includes:
- **Overview**: Purpose and use cases
- **Syntax**: Expression formats and builder patterns  
- **Parameters**: Detailed parameter specifications
- **Examples**: Practical usage scenarios
- **Error Handling**: Expected error conditions
- **Performance**: Optimization considerations
- **Best Practices**: Recommended usage patterns

For detailed information on any function, see the individual documentation files linked above.