# IndexOf Function Documentation

## Overview

The `IndexOf` function returns the index of the first occurrence of a specified substring within a string. It supports case-sensitive and case-insensitive searches with optional starting positions, making it essential for text parsing, validation, and data extraction workflows.

## Installation

### Extension Pack Registration
```csharp
// Register text functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterText();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Syntax

### Function Expression Formats
```json
// Basic search (case-sensitive)
"=indexOf('Hello World', 'World')"

// Search with starting index
"=indexOf('Hello World Hello', 'Hello', 6)"

// Case-insensitive search
"=indexOf('Hello World', 'WORLD', 0, true)"

// JSONPath source
"=indexOf($.text, $.searchValue)"
```

### Programmatic Usage
```csharp
// Basic search
var indexOfFunction = new IndexOf("text", "searchValue");

// With start index
var indexOfFunction = new IndexOf("text", "searchValue", "startIndex");

// With all options
var indexOfFunction = new IndexOf("text", "searchValue", "startIndex", "ignoreCase");

// Empty constructor for dynamic arguments
var indexOfFunction = new IndexOf();
```

### Builder Pattern
```csharp
var indexOfFunction = IndexOfBuilders.IndexOf("$.text", "search");
var withStart = IndexOfBuilders.IndexOf("$.text", "search", "5");
var caseInsensitive = IndexOfBuilders.IndexOf("$.text", "SEARCH", "0", "true");
```

## Parameters

- **text** (required): The source string to search in
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **searchValue** (required): The substring to search for
  - **Type**: String, JSONPath expression, or any value
  - **Empty String**: Returns 0 (found at beginning)
- **startIndex** (optional): Starting position for search (0-based)
  - **Type**: Integer or string representation of integer
  - **Default**: 0 (start from beginning)
  - **Validation**: Must be non-negative and within string bounds
- **ignoreCase** (optional): Whether to perform case-insensitive matching
  - **Type**: Boolean, string representation, or integer (0/1)
  - **Default**: false (case-sensitive)

## Return Value

- **Type**: Integer (JValue)
- **Value**: Index of first occurrence, or -1 if not found

## Examples

### Basic Substring Search
```json
{
  "path": "$.worldIndex",
  "value": "=indexOf('Hello World', 'World')",
  "command": "add"
}
```

**Result**:
```json
{
  "worldIndex": 6
}
```

### Not Found Result
```json
{
  "path": "$.notFoundIndex",
  "value": "=indexOf('Hello World', 'xyz')",
  "command": "add"
}
```

**Result**:
```json
{
  "notFoundIndex": -1
}
```

### Case-Insensitive Search
```json
{
  "path": "$.caseInsensitiveIndex",
  "value": "=indexOf('Hello World', 'WORLD', 0, true)",
  "command": "add"
}
```

**Result**:
```json
{
  "caseInsensitiveIndex": 6
}
```

### Search with Starting Position
```json
{
  "path": "$.secondOccurrence",
  "value": "=indexOf('Hello World Hello', 'Hello', 6)",
  "command": "add"
}
```

**Result**:
```json
{
  "secondOccurrence": 12
}
```

### Email Domain Extraction
```json
{
  "path": "$.atPosition",
  "value": "=indexOf($.email, '@')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "email": "user@example.com"
}
```

**Result**:
```json
{
  "email": "user@example.com",
  "atPosition": 4
}
```

### Array Processing for Text Analysis
```json
{
  "path": "$.logs[*].errorPosition",
  "value": "=indexOf(toLower(@.message), 'error')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "logs": [
    {"message": "Info: Process started"},
    {"message": "Error: File not found"},
    {"message": "Warning: Low memory"}
  ]
}
```

**Result**:
```json
{
  "logs": [
    {"message": "Info: Process started", "errorPosition": -1},
    {"message": "Error: File not found", "errorPosition": 0},
    {"message": "Warning: Low memory", "errorPosition": -1}
  ]
}
```

### String Parsing Pipeline
```json
[
  {
    "path": "$.colonIndex",
    "value": "=indexOf($.logEntry, ':')",
    "command": "add"
  },
  {
    "path": "$.timestamp",
    "value": "=substring($.logEntry, 0, $.colonIndex)",
    "command": "add"
  },
  {
    "path": "$.message",
    "value": "=substring($.logEntry, $.colonIndex + 2)",
    "command": "add"
  }
]
```

**Input Data**:
```json
{
  "logEntry": "2024-03-15 14:30:45: User login successful"
}
```

**Result**:
```json
{
  "logEntry": "2024-03-15 14:30:45: User login successful",
  "colonIndex": 19,
  "timestamp": "2024-03-15 14:30:45",
  "message": "User login successful"
}
```

### File Extension Detection
```json
{
  "path": "$.dotPosition",
  "value": "=indexOf($.filename, '.', -1, false)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filename": "document.backup.pdf"
}
```

**Result**:
```json
{
  "filename": "document.backup.pdf",
  "dotPosition": 8
}
```

## Advanced Usage

### Multi-delimiter Parsing
```csharp
var script = new JLioScript()
    .Add(IndexOfBuilders.IndexOf("@.data", ","))
    .OnPath("$.records[*].commaPos")
    .Add(IndexOfBuilders.IndexOf("@.data", ";"))
    .OnPath("$.records[*].semicolonPos")
    .Add(IndexOfBuilders.IndexOf("@.data", "|"))
    .OnPath("$.records[*].pipePos");
```

### URL Component Extraction
```csharp
var script = new JLioScript()
    .Add(IndexOfBuilders.IndexOf("$.url", "://"))
    .OnPath("$.protocolEnd")
    .Add(IndexOfBuilders.IndexOf("$.url", "/", "$.protocolEnd + 3"))
    .OnPath("$.domainEnd")
    .Add(IndexOfBuilders.IndexOf("$.url", "?"))
    .OnPath("$.queryStart");
```

### Log Analysis Pipeline
```json
{
  "path": "$.analysis",
  "value": {
    "hasError": "=indexOf(toLower($.logMessage), 'error') >= 0",
    "hasWarning": "=indexOf(toLower($.logMessage), 'warning') >= 0",
    "hasInfo": "=indexOf(toLower($.logMessage), 'info') >= 0"
  },
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=indexOf('hello world', 'world')"  // Result: 6
"=indexOf('hello world', 'WORLD')"  // Result: -1 (case-sensitive)
"=indexOf('hello world', 'WORLD', 0, true)"  // Result: 6 (case-insensitive)
```

### Number Conversion
```json
"=indexOf(12345, '3')"  // Result: 2 (from "12345")
```

### Empty String Searches
```json
"=indexOf('hello', '')"  // Result: 0 (empty string found at beginning)
"=indexOf('', 'hello')"  // Result: -1 (can't find in empty string)
```

### Out of Bounds Start Index
```json
"=indexOf('hello', 'l', 10)"  // Result: -1 (start index beyond string length)
```

## Fluent API Usage

### Basic Search
```csharp
var script = new JLioScript()
    .Add(IndexOfBuilders.IndexOf("$.text", "search"))
    .OnPath("$.position");
```

### Multiple Searches
```csharp
var script = new JLioScript()
    .Add(IndexOfBuilders.IndexOf("$.email", "@"))
    .OnPath("$.atPos")
    .Add(IndexOfBuilders.IndexOf("$.email", ".", "$.atPos"))
    .OnPath("$.dotPos");
```

### Conditional Search
```csharp
var script = new JLioScript()
    .Add(IndexOfBuilders.IndexOf("$.data", 
        "if(contains($.data, ','), ',', ';')", "0"))
    .OnPath("$.separatorPos");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=indexOf('text')"  // Logs error: "IndexOf requires 2 to 4 arguments"

// Too many arguments
"=indexOf('text', 'search', 0, true, 'extra')"  // Logs error: "IndexOf requires 2 to 4 arguments"
```

### Invalid Start Index
```json
// Non-numeric start index
"=indexOf('text', 'search', 'abc')"  // Logs error: "startIndex must be a non-negative integer"

// Negative start index
"=indexOf('text', 'search', -1)"  // Logs error: "startIndex must be a non-negative integer"
```

### Invalid Boolean Parameter
```json
// Invalid ignoreCase value
"=indexOf('text', 'search', 0, 'invalid')"  // Logs error: "ignoreCase must be a valid boolean"
```

## Performance Considerations

- **String Length**: Performance scales with source string length
- **Search Value Length**: Longer search strings may impact performance
- **Case Sensitivity**: Case-insensitive operations may be slightly slower
- **Start Index**: Larger start indices can improve performance by reducing search scope

## Best Practices

1. **Bounds Checking**: Validate start index is within string bounds
2. **Case Handling**: Choose appropriate case sensitivity for your use case
3. **Performance**: Use start index to limit search scope when possible
4. **Validation**: Check for -1 result before using index for substring operations
5. **Combine with Other Functions**: Chain with substring() for text extraction
6. **Testing**: Test with various input scenarios including edge cases

## Common Patterns

### Find and Extract Pattern
```json
"=if(indexOf($.text, ':') >= 0, substring($.text, indexOf($.text, ':') + 1), $.text)"
```

### Email Username Extraction Pattern
```json
"=substring($.email, 0, indexOf($.email, '@'))"
```

### File Extension Check Pattern
```json
"=indexOf(toLower($.filename), '.pdf') >= 0"
```

### Multiple Occurrence Pattern
```json
"=indexOf($.text, 'search', indexOf($.text, 'search') + 1)"
```

### Safe Substring Pattern
```json
"=if(indexOf($.text, ',') >= 0, substring($.text, 0, indexOf($.text, ',')), $.text)"
```

## Integration Examples

### With Substring Function
```json
"=substring($.text, indexOf($.text, ':') + 1)"  // Extract after delimiter
```

### With Conditional Logic
```json
"=if(indexOf($.text, 'error') >= 0, 'ERROR', 'OK')"  // Error detection
```

### With Replace Function
```json
"=replace($.text, substring($.text, 0, indexOf($.text, ' ')), 'REPLACED')"
```

### With Array Operations
```json
"$.items[?indexOf(@.name, 'important') >= 0]"  // Filter by content
```

## Comparison with Contains

| Function | Return Type | Use Case |
|----------|-------------|----------|
| **IndexOf** | Integer (position or -1) | Need exact position for extraction/parsing |
| **Contains** | Boolean (true/false) | Simple existence check for validation |

### Usage Decision
- Use **IndexOf** when you need the position for further text processing
- Use **Contains** when you only need to know if text exists