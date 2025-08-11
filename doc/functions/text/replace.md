# Replace Function Documentation

## Overview

The `Replace` function replaces all occurrences of a specified substring with another string. It supports both case-sensitive and case-insensitive replacements, making it essential for text cleaning, data normalization, and string transformation workflows.

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
// Basic replacement (case-sensitive)
"=replace('Hello World', 'World', 'JLio')"

// Case-insensitive replacement
"=replace('Hello WORLD', 'world', 'JLio', true)"

// JSONPath source
"=replace($.text, $.oldValue, $.newValue)"
```

### Programmatic Usage
```csharp
// Case-sensitive replacement
var replaceFunction = new Replace("text", "oldValue", "newValue");

// Case-insensitive replacement
var replaceFunction = new Replace("text", "oldValue", "newValue", "true");

// Empty constructor for dynamic arguments
var replaceFunction = new Replace();
```

### Builder Pattern
```csharp
var replaceFunction = ReplaceBuilders.Replace("$.text", "old", "new");
var caseInsensitive = ReplaceBuilders.Replace("$.text", "OLD", "new", "true");
```

## Parameters

- **text** (required): The source string to perform replacements on
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **oldValue** (required): The substring to find and replace
  - **Type**: String, JSONPath expression, or any value
  - **Validation**: Cannot be empty or null
- **newValue** (required): The replacement string
  - **Type**: String, JSONPath expression, or any value
  - **Empty Allowed**: Can be empty string for removal
- **ignoreCase** (optional): Whether to perform case-insensitive matching
  - **Type**: Boolean, string representation, or integer (0/1)
  - **Default**: false (case-sensitive)

## Return Value

- **Type**: String (JValue)
- **Value**: String with all occurrences replaced

## Examples

### Basic Text Replacement
```json
{
  "path": "$.greeting",
  "value": "=replace('Hello World', 'World', 'JLio')",
  "command": "add"
}
```

**Result**:
```json
{
  "greeting": "Hello JLio"
}
```

### Case-Insensitive Replacement
```json
{
  "path": "$.normalized",
  "value": "=replace('Hello WORLD', 'world', 'JLio', true)",
  "command": "add"
}
```

**Result**:
```json
{
  "normalized": "Hello JLio"
}
```

### Remove Text (Empty Replacement)
```json
{
  "path": "$.cleaned",
  "value": "=replace('Remove-this-text', '-this', '')",
  "command": "add"
}
```

**Result**:
```json
{
  "cleaned": "Remove-text"
}
```

### Multiple Replacements
```json
{
  "path": "$.processed",
  "value": "=replace('test test test', 'test', 'demo')",
  "command": "add"
}
```

**Result**:
```json
{
  "processed": "demo demo demo"
}
```

### JSONPath Source Data
```json
{
  "path": "$.cleanedEmail",
  "value": "=replace($.email, '@old-domain.com', '@new-domain.com')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "email": "user@old-domain.com"
}
```

**Result**:
```json
{
  "email": "user@old-domain.com",
  "cleanedEmail": "user@new-domain.com"
}
```

### Array Processing
```json
{
  "path": "$.users[*].normalizedName",
  "value": "=replace(@.name, 'Mr.', '')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"name": "Mr. John"},
    {"name": "Mr. Jane"},
    {"name": "Ms. Alice"}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"name": "Mr. John", "normalizedName": " John"},
    {"name": "Mr. Jane", "normalizedName": " Jane"},
    {"name": "Ms. Alice", "normalizedName": "Ms. Alice"}
  ]
}
```

### Data Cleaning Pipeline
```json
[
  {
    "path": "$.step1",
    "value": "=replace($.rawText, '  ', ' ')",
    "command": "add"
  },
  {
    "path": "$.step2",
    "value": "=replace($.step1, '\n', ' ')",
    "command": "add"
  },
  {
    "path": "$.cleaned",
    "value": "=trim($.step2)",
    "command": "add"
  }
]
```

### Case Normalization
```json
{
  "path": "$.platformName",
  "value": "=replace(toLower($.input), 'windows', 'Windows')",
  "command": "add"
}
```

## Advanced Usage

### Text Sanitization Pipeline
```csharp
var script = new JLioScript()
    .Add(ReplaceBuilders.Replace("$.userInput", "<", "&lt;"))
    .OnPath("$.step1")
    .Add(ReplaceBuilders.Replace("$.step1", ">", "&gt;"))
    .OnPath("$.step2")
    .Add(ReplaceBuilders.Replace("$.step2", "&", "&amp;"))
    .OnPath("$.sanitized");
```

### URL Slug Generation
```csharp
var script = new JLioScript()
    .Add(ToLowerBuilders.ToLower("$.title"))
    .OnPath("$.step1")
    .Add(ReplaceBuilders.Replace("$.step1", " ", "-"))
    .OnPath("$.step2")
    .Add(ReplaceBuilders.Replace("$.step2", "[^a-z0-9-]", "", "true"))
    .OnPath("$.slug");
```

### Data Format Conversion
```json
[
  {
    "path": "$.formattedPhone",
    "value": "=replace(replace(replace($.phone, '(', ''), ')', ''), '-', '')",
    "command": "add"
  }
]
```

## Data Type Handling

### String Values
```json
"=replace('hello world', 'world', 'JLio')"  // Result: "hello JLio"
```

### Number Conversion
```json
"=replace(12345, '3', 'X')"  // Result: "12X45" (from "12345")
```

### Boolean Values
```json
"=replace(true, 'true', 'yes')"  // Result: "yes"
```

### Case Sensitivity
```json
"=replace('Hello', 'HELLO', 'Hi', false)"  // Result: "Hello" (no match)
"=replace('Hello', 'HELLO', 'Hi', true)"   // Result: "Hi" (case ignored)
```

## Fluent API Usage

### Basic Replacement
```csharp
var script = new JLioScript()
    .Add(ReplaceBuilders.Replace("$.text", "old", "new"))
    .OnPath("$.updated");
```

### Multiple Replacements
```csharp
var script = new JLioScript()
    .Add(ReplaceBuilders.Replace("$.text", " ", "_"))
    .OnPath("$.step1")
    .Add(ReplaceBuilders.Replace("$.step1", "-", "_"))
    .OnPath("$.normalized");
```

### Conditional Replacement
```csharp
var script = new JLioScript()
    .Add(ReplaceBuilders.Replace("$.description", "TODO", "COMPLETED"))
    .OnPath("$.tasks[?(@.status == 'done')].description");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=replace('text', 'old')"  // Logs error: "Replace requires 3 or 4 arguments"

// Too many arguments
"=replace('text', 'old', 'new', true, 'extra')"  // Logs error: "Replace requires 3 or 4 arguments"
```

### Empty Old Value
```json
// Empty oldValue
"=replace('text', '', 'new')"  // Logs error: "Replace oldValue cannot be empty"
```

### Invalid Boolean Parameter
```json
// Invalid ignoreCase value
"=replace('text', 'old', 'new', 'invalid')"  // Logs error: "ignoreCase must be a valid boolean"
```

## Performance Considerations

- **String Length**: Performance scales with source string length and number of occurrences
- **Case Sensitivity**: Case-insensitive operations may be slightly slower
- **Pattern Complexity**: Simple string replacements are optimized
- **Memory Usage**: Large strings and many replacements require memory allocation

## Best Practices

1. **Validation**: Ensure oldValue is not empty
2. **Case Handling**: Choose appropriate case sensitivity for your use case
3. **Chaining**: Chain multiple replace operations for complex transformations
4. **Performance**: Use case-sensitive matching when possible for better performance
5. **Testing**: Test with various input scenarios including edge cases
6. **Escaping**: Consider escaping special characters in replacement strings

## Common Patterns

### Text Cleaning Pattern
```json
"=replace(trim($.input), '  ', ' ')"  // Remove extra spaces
```

### Data Sanitization Pattern
```json
"=replace(replace($.input, '<', '&lt;'), '>', '&gt;')"  // HTML escape
```

### Format Normalization Pattern
```json
"=replace(replace($.phone, '(', ''), ')', '')"  // Clean phone format
```

### Case Insensitive Cleanup Pattern
```json
"=replace($.text, 'remove me', '', true)"  // Remove regardless of case
```

### URL-Safe Conversion Pattern
```json
"=replace(toLower($.title), ' ', '-')"  // Create URL slug
```