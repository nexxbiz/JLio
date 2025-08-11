# Contains Function Documentation

## Overview

The `Contains` function checks whether a string contains a specified substring. It supports both case-sensitive and case-insensitive searching, making it essential for text validation, filtering, and conditional logic workflows.

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
// Basic contains check (case-sensitive)
"=contains('Hello World', 'World')"

// Case-insensitive check
"=contains('Hello World', 'WORLD', true)"

// JSONPath source
"=contains($.email, '@gmail.com')"
```

### Programmatic Usage
```csharp
// Case-sensitive contains
var containsFunction = new Contains("text", "searchValue");

// Case-insensitive contains
var containsFunction = new Contains("text", "searchValue", "true");

// Empty constructor for dynamic arguments
var containsFunction = new Contains();
```

### Builder Pattern
```csharp
var containsFunction = ContainsBuilders.Contains("$.text", "search");
var caseInsensitive = ContainsBuilders.Contains("$.text", "SEARCH", "true");
```

## Parameters

- **text** (required): The source string to search in
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **searchValue** (required): The substring to search for
  - **Type**: String, JSONPath expression, or any value
  - **Empty Allowed**: Empty string will always return true
- **ignoreCase** (optional): Whether to perform case-insensitive matching
  - **Type**: Boolean, string representation, or integer (0/1)
  - **Default**: false (case-sensitive)

## Return Value

- **Type**: Boolean (JValue)
- **Value**: true if substring is found, false otherwise

## Examples

### Basic Contains Check
```json
{
  "path": "$.hasWorld",
  "value": "=contains('Hello World', 'World')",
  "command": "add"
}
```

**Result**:
```json
{
  "hasWorld": true
}
```

### Case-Insensitive Check
```json
{
  "path": "$.hasWorld",
  "value": "=contains('Hello World', 'WORLD', true)",
  "command": "add"
}
```

**Result**:
```json
{
  "hasWorld": true
}
```

### Email Domain Check
```json
{
  "path": "$.isGmail",
  "value": "=contains($.email, '@gmail.com')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "email": "user@gmail.com"
}
```

**Result**:
```json
{
  "email": "user@gmail.com",
  "isGmail": true
}
```

### Array Filtering with Contains
```json
{
  "path": "$.gmailUsers",
  "value": "$.users[?contains(@.email, '@gmail.com')]",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"name": "Alice", "email": "alice@gmail.com"},
    {"name": "Bob", "email": "bob@yahoo.com"},
    {"name": "Charlie", "email": "charlie@gmail.com"}
  ]
}
```

**Result**:
```json
{
  "users": [...],
  "gmailUsers": [
    {"name": "Alice", "email": "alice@gmail.com"},
    {"name": "Charlie", "email": "charlie@gmail.com"}
  ]
}
```

### Conditional Processing
```json
{
  "path": "$.category",
  "value": "=if(contains(toLower($.description), 'urgent'), 'high-priority', 'normal')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "description": "This is an URGENT task that needs attention"
}
```

**Result**:
```json
{
  "description": "This is an URGENT task that needs attention",
  "category": "high-priority"
}
```

### Multiple Keyword Detection
```json
[
  {
    "path": "$.hasError",
    "value": "=contains(toLower($.logMessage), 'error')",
    "command": "add"
  },
  {
    "path": "$.hasWarning",
    "value": "=contains(toLower($.logMessage), 'warning')",
    "command": "add"
  },
  {
    "path": "$.hasInfo",
    "value": "=contains(toLower($.logMessage), 'info')",
    "command": "add"
  }
]
```

## Advanced Usage

### Content Classification
```csharp
var script = new JLioScript()
    .Add(ContainsBuilders.Contains("@.content", "javascript", "true"))
    .OnPath("$.posts[*].isJavaScript")
    .Add(ContainsBuilders.Contains("@.content", "python", "true"))
    .OnPath("$.posts[*].isPython")
    .Add(ContainsBuilders.Contains("@.content", "c#", "true"))
    .OnPath("$.posts[*].isCSharp");
```

### Security Validation
```csharp
var script = new JLioScript()
    .Add(ContainsBuilders.Contains("@.userInput", "<script", "true"))
    .OnPath("$.inputs[*].hasScript")
    .Add(ContainsBuilders.Contains("@.userInput", "javascript:", "true"))
    .OnPath("$.inputs[*].hasJSProtocol");
```

### Tag-based Filtering
```json
{
  "path": "$.taggedItems",
  "value": "$.items[?contains(@.tags, 'important')]",
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=contains('hello world', 'world')"  // Result: true
"=contains('hello world', 'WORLD')"  // Result: false (case-sensitive)
"=contains('hello world', 'WORLD', true)"  // Result: true (case-insensitive)
```

### Number Conversion
```json
"=contains(12345, '23')"  // Result: true (converted from "12345")
```

### Empty String Searches
```json
"=contains('hello', '')"  // Result: true (empty string is always contained)
"=contains('', 'hello')"  // Result: false (can't find in empty string)
```

### Case Sensitivity Comparison
```json
// Case-sensitive (default)
"=contains('Hello', 'hello')"  // Result: false

// Case-insensitive
"=contains('Hello', 'hello', true)"  // Result: true
```

## Fluent API Usage

### Basic Contains Check
```csharp
var script = new JLioScript()
    .Add(ContainsBuilders.Contains("$.description", "urgent"))
    .OnPath("$.isUrgent");
```

### Multiple Checks
```csharp
var script = new JLioScript()
    .Add(ContainsBuilders.Contains("$.filename", ".pdf", "true"))
    .OnPath("$.isPdf")
    .Add(ContainsBuilders.Contains("$.filename", ".doc", "true"))
    .OnPath("$.isDoc")
    .Add(ContainsBuilders.Contains("$.filename", ".txt", "true"))
    .OnPath("$.isText");
```

### Validation Pipeline
```csharp
var script = new JLioScript()
    .Add(ContainsBuilders.Contains("$.email", "@"))
    .OnPath("$.hasAtSymbol")
    .Add(ContainsBuilders.Contains("$.email", ".com"))
    .OnPath("$.hasComDomain");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=contains('text')"  // Logs error: "Contains requires 2 or 3 arguments"

// Too many arguments
"=contains('text', 'search', true, 'extra')"  // Logs error: "Contains requires 2 or 3 arguments"
```

### Invalid Boolean Parameter
```json
// Invalid ignoreCase value
"=contains('text', 'search', 'invalid')"  // Logs error: "ignoreCase must be a valid boolean"
```

## Performance Considerations

- **String Length**: Performance scales with source string length
- **Case Sensitivity**: Case-insensitive operations may be slightly slower
- **Search Value Length**: Longer search strings may impact performance
- **Memory Usage**: Minimal additional memory overhead

## Best Practices

1. **Case Handling**: Choose appropriate case sensitivity for your use case
2. **Performance**: Use case-sensitive matching when possible for better performance
3. **Empty Checks**: Remember that empty search strings always return true
4. **Validation**: Use for input validation and content filtering
5. **Combine with Other Functions**: Chain with trim() or toLower() for robust checks
6. **Testing**: Test with various input scenarios including edge cases

## Common Patterns

### Email Validation Pattern
```json
"=contains($.email, '@') && contains($.email, '.')"
```

### Keyword Search Pattern
```json
"=contains(toLower($.content), toLower($.searchTerm))"
```

### File Extension Check Pattern
```json
"=contains($.filename, '.pdf', true)"
```

### Security Validation Pattern
```json
"=!contains(toLower($.input), '<script')"
```

### Content Classification Pattern
```json
"=if(contains($.content, 'TODO', true), 'incomplete', 'complete')"
```

## Integration with Other Functions

### With Conditional Logic
```json
"=if(contains($.status, 'error'), 'failed', 'success')"
```

### With String Normalization
```json
"=contains(trim(toLower($.input)), 'keyword')"
```

### With Array Operations
```json
"$.items[?contains(@.description, 'important')]"
```