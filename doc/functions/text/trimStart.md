# TrimStart Function Documentation

## Overview

The `TrimStart` function removes whitespace characters or specified characters from the beginning (left side) of a string. It's essential for data cleaning, user input processing, and text normalization workflows where leading characters need to be removed.

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
// Remove whitespace from beginning
"=trimStart('  Hello World')"

// Remove specific characters from beginning
"=trimStart('---Hello World', '-')"

// JSONPath source
"=trimStart($.userInput)"
```

### Programmatic Usage
```csharp
// Default whitespace trimming
var trimStartFunction = new TrimStart("text");

// Custom character trimming
var trimStartFunction = new TrimStart("text", "characters");

// Empty constructor for dynamic arguments
var trimStartFunction = new TrimStart();
```

### Builder Pattern
```csharp
var trimStartFunction = TrimStartBuilders.TrimStart("$.text");
var customTrim = TrimStartBuilders.TrimStart("$.text", ".,!?");
```

## Parameters

- **text** (required): The source string to trim
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **trimChars** (optional): Characters to remove from the beginning
  - **Type**: String containing characters to trim
  - **Default**: Whitespace characters (spaces, tabs, newlines, etc.)
  - **Usage**: Each character in the string is used as a trim character

## Return Value

- **Type**: String (JValue)
- **Value**: String with specified characters removed from the beginning

## Examples

### Basic Whitespace Trimming
```json
{
  "path": "$.cleaned",
  "value": "=trimStart('   Hello World   ')",
  "command": "add"
}
```

**Result**:
```json
{
  "cleaned": "Hello World   "
}
```

### Custom Character Trimming
```json
{
  "path": "$.trimmed",
  "value": "=trimStart('---Hello World---', '-')",
  "command": "add"
}
```

**Result**:
```json
{
  "trimmed": "Hello World---"
}
```

### Multiple Character Trimming
```json
{
  "path": "$.cleaned",
  "value": "=trimStart('.,!Hello World!,.', '.!,')",
  "command": "add"
}
```

**Result**:
```json
{
  "cleaned": "Hello World!,."
}
```

### Code Comment Processing
```json
{
  "path": "$.codeWithoutComment",
  "value": "=trimStart($.codeLine, '// ')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "codeLine": "// This is a comment"
}
```

**Result**:
```json
{
  "codeLine": "// This is a comment",
  "codeWithoutComment": "This is a comment"
}
```

### Array Processing for Log Cleanup
```json
{
  "path": "$.logs[*].cleanMessage",
  "value": "=trimStart(@.message, '[INFO] [WARNING] [ERROR] ')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "logs": [
    {"message": "[INFO] System started"},
    {"message": "[ERROR] File not found"},
    {"message": "[WARNING] Low memory"}
  ]
}
```

**Result**:
```json
{
  "logs": [
    {"message": "[INFO] System started", "cleanMessage": "System started"},
    {"message": "[ERROR] File not found", "cleanMessage": "File not found"},
    {"message": "[WARNING] Low memory", "cleanMessage": "Low memory"}
  ]
}
```

### Leading Zero Removal
```json
{
  "path": "$.numberWithoutLeadingZeros",
  "value": "=trimStart($.paddedNumber, '0')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "paddedNumber": "00012345"
}
```

**Result**:
```json
{
  "paddedNumber": "00012345",
  "numberWithoutLeadingZeros": "12345"
}
```

### URL Protocol Removal
```json
{
  "path": "$.domainOnly",
  "value": "=trimStart($.url, 'https://http://')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "url": "https://www.example.com"
}
```

**Result**:
```json
{
  "url": "https://www.example.com",
  "domainOnly": "www.example.com"
}
```

### Data Validation Pipeline
```json
[
  {
    "path": "$.step1",
    "value": "=trimStart($.rawInput)",
    "command": "add"
  },
  {
    "path": "$.step2",
    "value": "=trimStart($.step1, '#-*')",
    "command": "add"
  },
  {
    "path": "$.isValid",
    "value": "=length($.step2) > 0",
    "command": "add"
  }
]
```

## Advanced Usage

### Multi-step Data Cleaning
```csharp
var script = new JLioScript()
    .Add(TrimStartBuilders.TrimStart("$.userInput"))
    .OnPath("$.step1")
    .Add(TrimStartBuilders.TrimStart("$.step1", "0"))
    .OnPath("$.step2")
    .Add(TrimStartBuilders.TrimStart("$.step2", "#*-"))
    .OnPath("$.cleaned");
```

### Log Entry Processing
```csharp
var script = new JLioScript()
    .Add(TrimStartBuilders.TrimStart("@.logEntry", "[INFO][ERROR][WARNING]"))
    .OnPath("$.logs[*].message")
    .Add(TrimStartBuilders.TrimStart("@.message", " \t"))
    .OnPath("$.logs[*].cleanMessage");
```

### CSV Field Cleaning
```json
{
  "path": "$.cleanedFields",
  "value": "=split($.csvRow, ',').map(x => trimStart(x, ' \"'))",
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=trimStart('  hello')"  // Result: "hello"
```

### Number Conversion
```json
"=trimStart(000123, '0')"  // Result: "123" (converted from "000123")
```

### Empty String
```json
"=trimStart('')"  // Result: "" (empty string)
"=trimStart('   ')"  // Result: "" (only whitespace)
```

### Null Values
```json
"=trimStart($.missing)"  // Result: "" (null treated as empty)
```

## Fluent API Usage

### Basic Left Trimming
```csharp
var script = new JLioScript()
    .Add(TrimStartBuilders.TrimStart("$.input"))
    .OnPath("$.cleaned");
```

### Custom Character Trimming
```csharp
var script = new JLioScript()
    .Add(TrimStartBuilders.TrimStart("$.code", "//"))
    .OnPath("$.uncommented");
```

### Bulk Data Cleaning
```csharp
var script = new JLioScript()
    .Add(TrimStartBuilders.TrimStart("@.name"))
    .OnPath("$.records[*].name")
    .Add(TrimStartBuilders.TrimStart("@.description", "- "))
    .OnPath("$.records[*].description");
```

## Related Functions

### Trim (Both Sides)
```json
"=trim('  hello  ')"  // Result: "hello" (both sides trimmed)
```

### TrimEnd (Right Side)
```json
"=trimEnd('  hello  ')"  // Result: "  hello" (only right side trimmed)
```

### Combined Usage
```json
{
  "path": "$.fullyProcessed",
  "value": "=trimEnd(trimStart($.input, '-'), '.')",
  "command": "add"
}
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=trimStart()"  // Logs error: "TrimStart requires 1 or 2 arguments"

// Too many arguments
"=trimStart('text', 'chars', 'extra')"  // Logs error: "TrimStart requires 1 or 2 arguments"
```

## Performance Considerations

- **String Length**: Performance scales with source string length
- **Character Set**: Larger trim character sets may impact performance slightly
- **Memory Usage**: Creates new string instances for results
- **Whitespace Detection**: Default whitespace trimming is optimized

## Best Practices

1. **Input Validation**: Use trimStart for cleaning user inputs
2. **Chain Operations**: Combine with other string functions for complete processing
3. **Custom Characters**: Use specific trim characters for structured data
4. **Null Safety**: Remember that null values become empty strings
5. **Performance**: Use default whitespace trimming when possible
6. **Testing**: Test with various input scenarios including edge cases

## Common Patterns

### User Input Sanitization Pattern
```json
"=trimStart($.userInput)"  // Basic input cleaning
```

### Leading Character Removal Pattern
```json
"=trimStart($.code, '// ')"  // Remove comment markers
```

### Padding Removal Pattern
```json
"=trimStart($.paddedId, '0')"  // Remove leading zeros
```

### Protocol Stripping Pattern
```json
"=trimStart($.url, 'http://https://')"  // Remove URL protocols
```

### Multi-character Cleanup Pattern
```json
"=trimStart($.text, ' \t\n\r#-*')"  // Remove various unwanted leading characters
```

## Integration Examples

### With String Functions
```json
"=toUpper(trimStart($.text))"  // Trim then uppercase
```

### With Conditional Logic
```json
"=if(length(trimStart($.field)) > 0, trimStart($.field), 'EMPTY')"
```

### With Replace Function
```json
"=replace(trimStart($.text, '#'), 'old', 'new')"
```

### With Validation
```json
"=length(trimStart($.requiredField)) == 0"  // Check for empty after trimming
```