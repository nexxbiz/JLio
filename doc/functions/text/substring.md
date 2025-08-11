# Substring Function Documentation

## Overview

The `Substring` function extracts a portion of a string starting at a specified index with an optional length parameter. It supports negative indices for counting from the end of the string and provides robust bounds checking.

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
// Extract from start index to end of string
"=substring('Hello World', 6)"

// Extract specific length from start index
"=substring('Hello World', 0, 5)"

// Use negative index to count from end
"=substring('Hello World', -5)"

// Extract from JSONPath expression
"=substring($.text, 2, 8)"
```

### Programmatic Usage
```csharp
// With start index only
var substringFunction = new Substring("text", "startIndex");

// With start index and length
var substringFunction = new Substring("text", "startIndex", "length");

// Empty constructor for dynamic arguments
var substringFunction = new Substring();
```

### Builder Pattern
```csharp
var substringFunction = SubstringBuilders.Substring("$.text", "0", "5");
var fromEnd = SubstringBuilders.Substring("$.text", "-3");
```

## Parameters

- **text** (required): The source string to extract from
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **startIndex** (required): Starting position (0-based indexing)
  - **Type**: Integer or string representation of integer
  - **Negative Values**: Count from end of string (-1 = last character)
- **length** (optional): Number of characters to extract
  - **Type**: Integer or string representation of integer
  - **Default**: Extract to end of string
  - **Bounds**: Automatically capped to available characters

## Return Value

- **Type**: String (JValue)
- **Value**: Extracted substring

## Examples

### Basic Substring Extraction
```json
{
  "path": "$.greeting",
  "value": "=substring('Hello World', 0, 5)",
  "command": "add"
}
```

**Result**:
```json
{
  "greeting": "Hello"
}
```

### Extract to End of String
```json
{
  "path": "$.world",
  "value": "=substring('Hello World', 6)",
  "command": "add"
}
```

**Result**:
```json
{
  "world": "World"
}
```

### Negative Index (from end)
```json
{
  "path": "$.lastThree",
  "value": "=substring('Hello World', -5)",
  "command": "add"
}
```

**Result**:
```json
{
  "lastThree": "World"
}
```

### JSONPath Source
```json
{
  "path": "$.firstName",
  "value": "=substring($.fullName, 0, indexOf($.fullName, ' '))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "fullName": "John Doe Smith"
}
```

**Result**:
```json
{
  "fullName": "John Doe Smith",
  "firstName": "John"
}
```

### Array Processing
```json
{
  "path": "$.users[*].initials",
  "value": "=substring(@.name, 0, 1)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"name": "Alice"},
    {"name": "Bob"},
    {"name": "Charlie"}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"name": "Alice", "initials": "A"},
    {"name": "Bob", "initials": "B"},
    {"name": "Charlie", "initials": "C"}
  ]
}
```

### Text Truncation
```json
{
  "path": "$.summary",
  "value": "=if(length($.description) > 50, concat(substring($.description, 0, 47), '...'), $.description)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "description": "This is a very long description that needs to be truncated for display purposes"
}
```

**Result**:
```json
{
  "description": "This is a very long description that needs to be truncated for display purposes",
  "summary": "This is a very long description that needs to..."
}
```

### Extract File Extension
```json
{
  "path": "$.extension",
  "value": "=substring($.filename, indexOf($.filename, '.') + 1)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filename": "document.pdf"
}
```

**Result**:
```json
{
  "filename": "document.pdf",
  "extension": "pdf"
}
```

## Advanced Usage

### String Parsing Pipeline
```csharp
var script = new JLioScript()
    .Add(SubstringBuilders.Substring("$.email", "0", "indexOf($.email, '@')"))
    .OnPath("$.username")
    .Add(SubstringBuilders.Substring("$.email", "indexOf($.email, '@') + 1"))
    .OnPath("$.domain");
```

### Text Processing with Bounds
```json
{
  "path": "$.safeSubstring",
  "value": "=substring($.text, 5, min(10, length($.text) - 5))",
  "command": "add"
}
```

### Multi-level Extraction
```csharp
var script = new JLioScript()
    .Add(SubstringBuilders.Substring("@.fullPath", "lastIndexOf(@.fullPath, '/') + 1"))
    .OnPath("$.files[*].filename")
    .Add(SubstringBuilders.Substring("@.filename", "0", "lastIndexOf(@.filename, '.')"))
    .OnPath("$.files[*].basename");
```

## Data Type Handling

### String Values
```json
"=substring('Hello', 1, 3)"  // Result: "ell"
```

### Number Conversion
```json
"=substring(12345, 1, 2)"  // Result: "23" (converted from "12345")
```

### Empty String
```json
"=substring('', 0, 5)"  // Result: "" (empty string)
```

### Bounds Handling
```json
"=substring('Hi', 0, 10)"  // Result: "Hi" (length capped to available)
"=substring('Hello', 10)"   // Error: index out of bounds
```

## Fluent API Usage

### Basic Substring
```csharp
var script = new JLioScript()
    .Add(SubstringBuilders.Substring("$.text", "0", "10"))
    .OnPath("$.preview");
```

### Multiple Extractions
```csharp
var script = new JLioScript()
    .Add(SubstringBuilders.Substring("$.fullName", "0", "indexOf($.fullName, ' ')"))
    .OnPath("$.firstName")
    .Add(SubstringBuilders.Substring("$.fullName", "indexOf($.fullName, ' ') + 1"))
    .OnPath("$.lastName");
```

### Conditional Substring
```csharp
var script = new JLioScript()
    .Add(SubstringBuilders.Substring("$.description", "0", "if(length($.description) > 100, 100, length($.description))"))
    .OnPath("$.truncatedDescription");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=substring('text')"  // Logs error: "Substring requires 2 or 3 arguments"

// Too many arguments
"=substring('text', 0, 5, 'extra')"  // Logs error: "Substring requires 2 or 3 arguments"
```

### Invalid Index Values
```json
// Non-integer index
"=substring('text', 'abc', 2)"  // Logs error: "startIndex must be a valid integer"

// Negative length
"=substring('text', 0, -1)"  // Logs error: "length cannot be negative"
```

### Index Out of Bounds
```json
// Start index beyond string length
"=substring('Hi', 5)"  // Logs error: "startIndex 5 is out of bounds for string of length 2"
```

## Performance Considerations

- **String Length**: Performance scales with source string length
- **Index Calculations**: Complex index expressions may impact performance
- **Bounds Checking**: Validation adds minimal overhead
- **Memory Usage**: Substring creation requires memory allocation

## Best Practices

1. **Bounds Validation**: Always validate indices before use
2. **Negative Index Usage**: Use negative indices for end-relative positioning
3. **Length Checking**: Combine with length() for safe operations
4. **Error Handling**: Monitor execution logs for index errors
5. **Performance**: Use direct indices when possible rather than calculations
6. **Testing**: Test with edge cases including empty strings and boundary conditions

## Common Patterns

### Safe Substring Pattern
```json
"=if(length($.text) >= 10, substring($.text, 0, 10), $.text)"
```

### Extract Until Delimiter Pattern
```json
"=substring($.text, 0, indexOf($.text, ','))"
```

### Extract After Delimiter Pattern
```json
"=substring($.text, indexOf($.text, ',') + 1)"
```

### Truncate with Ellipsis Pattern
```json
"=if(length($.text) > 20, concat(substring($.text, 0, 17), '...'), $.text)"
```

### File Path Parsing Pattern
```json
"=substring($.path, lastIndexOf($.path, '/') + 1)"  // Extract filename
```