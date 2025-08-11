# Trim Function Documentation

## Overview

The `Trim` function removes whitespace characters or specified characters from both the beginning and end of a string. It's essential for data cleaning, user input processing, and string normalization workflows.

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
// Remove whitespace from both ends
"=trim('  Hello World  ')"

// Remove specific characters from both ends
"=trim('---Hello World---', '-')"

// JSONPath source
"=trim($.userInput)"
```

### Programmatic Usage
```csharp
// Default whitespace trimming
var trimFunction = new Trim("text");

// Custom character trimming
var trimFunction = new Trim("text", "characters");

// Empty constructor for dynamic arguments
var trimFunction = new Trim();
```

### Builder Pattern
```csharp
var trimFunction = TrimBuilders.Trim("$.text");
var customTrim = TrimBuilders.Trim("$.text", ".,!?");
```

## Parameters

- **text** (required): The source string to trim
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **trimChars** (optional): Characters to remove from both ends
  - **Type**: String containing characters to trim
  - **Default**: Whitespace characters (spaces, tabs, newlines, etc.)
  - **Usage**: Each character in the string is used as a trim character

## Return Value

- **Type**: String (JValue)
- **Value**: String with specified characters removed from both ends

## Examples

### Basic Whitespace Trimming
```json
{
  "path": "$.cleaned",
  "value": "=trim('   Hello World   ')",
  "command": "add"
}
```

**Result**:
```json
{
  "cleaned": "Hello World"
}
```

### Custom Character Trimming
```json
{
  "path": "$.trimmed",
  "value": "=trim('---Hello World---', '-')",
  "command": "add"
}
```

**Result**:
```json
{
  "trimmed": "Hello World"
}
```

### Multiple Character Trimming
```json
{
  "path": "$.cleaned",
  "value": "=trim('.,!Hello World!,.', '.!,')",
  "command": "add"
}
```

**Result**:
```json
{
  "cleaned": "Hello World"
}
```

### JSONPath User Input Cleaning
```json
{
  "path": "$.cleanedInput",
  "value": "=trim($.userInput)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "userInput": "  John Doe  \n"
}
```

**Result**:
```json
{
  "userInput": "  John Doe  \n",
  "cleanedInput": "John Doe"
}
```

### Array Processing
```json
{
  "path": "$.users[*].cleanName",
  "value": "=trim(@.name)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"name": "  Alice  "},
    {"name": "\tBob\n"},
    {"name": " Charlie "}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"name": "  Alice  ", "cleanName": "Alice"},
    {"name": "\tBob\n", "cleanName": "Bob"},
    {"name": " Charlie ", "cleanName": "Charlie"}
  ]
}
```

### Data Validation Pipeline
```json
[
  {
    "path": "$.step1",
    "value": "=trim($.rawData)",
    "command": "add"
  },
  {
    "path": "$.isValid",
    "value": "=length($.step1) > 0",
    "command": "add"
  }
]
```

## Advanced Usage

### Multi-step Data Cleaning
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("$.userInput"))
    .OnPath("$.step1")
    .Add(ReplaceBuilders.Replace("$.step1", "  ", " "))
    .OnPath("$.step2")
    .Add(TrimBuilders.Trim("$.step2"))
    .OnPath("$.cleaned");
```

### Form Data Processing
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("@.firstName"))
    .OnPath("$.formData[*].firstName")
    .Add(TrimBuilders.Trim("@.lastName"))
    .OnPath("$.formData[*].lastName")
    .Add(TrimBuilders.Trim("@.email"))
    .OnPath("$.formData[*].email");
```

### Special Character Cleanup
```json
{
  "path": "$.cleanedCode",
  "value": "=trim($.rawCode, ' \t\n\r;,')",
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=trim('  hello  ')"  // Result: "hello"
```

### Number Conversion
```json
"=trim(123)"  // Result: "123" (no trimming needed)
```

### Empty String
```json
"=trim('')"  // Result: "" (empty string)
"=trim('   ')"  // Result: "" (only whitespace)
```

### Null Values
```json
"=trim($.missing)"  // Result: "" (null treated as empty)
```

## Fluent API Usage

### Basic Trimming
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("$.input"))
    .OnPath("$.cleaned");
```

### Custom Character Trimming
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("$.filename", "."))
    .OnPath("$.basename");
```

### Bulk Data Cleaning
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("@.name"))
    .OnPath("$.records[*].name")
    .Add(TrimBuilders.Trim("@.description"))
    .OnPath("$.records[*].description");
```

## Related Functions

### TrimStart (TrimLeft)
```json
"=trimStart('  hello  ')"  // Result: "hello  " (only left side trimmed)
```

### TrimEnd (TrimRight)
```json
"=trimEnd('  hello  ')"  // Result: "  hello" (only right side trimmed)
```

### Combined Usage
```json
{
  "path": "$.processed",
  "value": "=trim(replace($.input, '\n', ' '))",
  "command": "add"
}
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=trim()"  // Logs error: "Trim requires 1 or 2 arguments"

// Too many arguments
"=trim('text', 'chars', 'extra')"  // Logs error: "Trim requires 1 or 2 arguments"
```

## Performance Considerations

- **String Length**: Performance scales with source string length
- **Character Set**: Larger trim character sets may impact performance slightly
- **Memory Usage**: Creates new string instances for results
- **Whitespace Detection**: Default whitespace trimming is optimized

## Best Practices

1. **Input Validation**: Always trim user inputs for data quality
2. **Chain Operations**: Combine with other string functions for complete cleaning
3. **Custom Characters**: Use specific trim characters when dealing with formatted data
4. **Null Safety**: Remember that null values become empty strings
5. **Performance**: Use default whitespace trimming when possible
6. **Testing**: Test with various input scenarios including edge cases

## Common Patterns

### User Input Sanitization Pattern
```json
"=trim($.userInput)"  // Basic input cleaning
```

### Double Trimming Pattern
```json
"=trim(replace($.text, '  ', ' '))"  // Remove extra spaces then trim
```

### Custom Delimiter Trimming Pattern
```json
"=trim($.csvField, ' \"')"  // Remove quotes and spaces from CSV field
```

### Validation Preparation Pattern
```json
"=length(trim($.requiredField)) > 0"  // Check for non-empty content
```

### Multi-character Cleanup Pattern
```json
"=trim($.code, ' \t\n\r;,')"  // Remove various unwanted characters
```